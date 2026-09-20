using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Auth;

/// <summary>
/// 基于 HttpClient 调用 AuthServer /connect/token 与 /connect/revocation 的实现。
/// </summary>
/// <remarks>
/// 职责边界：只负责协议交互与错误映射，不做任何业务判断；
/// 业务编排（补用户信息、组装返回结果）由 <see cref="AuthAppService"/> 完成。
/// </remarks>
public class AuthTokenClient : IAuthTokenClient, ITransientDependency
{
    /// <summary>
    /// HttpApi.Host 注册的命名 HttpClient 名称，用于统一超时与拦截器配置。
    /// </summary>
    public const string HttpClientName = "ProjectAuthTokenClient";

    /// <summary>
    /// OAuth2 响应字段名大小写不敏感，统一复用同一份序列化配置。
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthTokenOptions _options;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// 属性注入的日志器，便于排查 AuthServer 返回的具体错误。
    /// </summary>
    public ILogger<AuthTokenClient> Logger { get; set; }

    /// <summary>
    /// 构造令牌客户端。
    /// </summary>
    /// <param name="httpClientFactory">命名 HttpClient 工厂，由 Host 模块注册。</param>
    /// <param name="options">AuthServer 连接配置。</param>
    /// <param name="currentTenant">当前租户，用于向 AuthServer 透传 __tenant。</param>
    public AuthTokenClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthTokenOptions> options,
        ICurrentTenant currentTenant)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _currentTenant = currentTenant;

        Logger = NullLogger<AuthTokenClient>.Instance;
    }

    /// <summary>
    /// 按 password 模式申请令牌。
    /// </summary>
    /// <param name="userNameOrEmailAddress">用户名或邮箱地址。</param>
    /// <param name="password">明文密码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>令牌终结点的原始响应。</returns>
    public virtual Task<TokenResponse> RequestPasswordTokenAsync(
        string userNameOrEmailAddress,
        string password,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            [OpenIddictConstants.Parameters.GrantType] = AuthConsts.PasswordGrantType,
            [OpenIddictConstants.Parameters.ClientId] = _options.ClientId,
            [OpenIddictConstants.Parameters.ClientSecret] = _options.ClientSecret,
            [OpenIddictConstants.Parameters.Username] = userNameOrEmailAddress,
            [OpenIddictConstants.Parameters.Password] = password,
            [OpenIddictConstants.Parameters.Scope] = _options.Scope
        };

        // 凭据类错误（invalid_grant）映射为"用户名或密码错误"，其余归为令牌请求失败
        return RequestTokenAsync(
            parameters,
            ProjectDomainErrorCodes.InvalidUserNameOrPassword,
            ProjectDomainErrorCodes.TokenRequestFailed,
            cancellationToken);
    }

    /// <summary>
    /// 按 refresh_token 模式换取新的访问令牌。
    /// </summary>
    /// <param name="refreshToken">上一次下发的刷新令牌。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>令牌终结点的原始响应。</returns>
    public virtual Task<TokenResponse> RequestRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            [OpenIddictConstants.Parameters.GrantType] = AuthConsts.RefreshTokenGrantType,
            [OpenIddictConstants.Parameters.ClientId] = _options.ClientId,
            [OpenIddictConstants.Parameters.ClientSecret] = _options.ClientSecret,
            [OpenIddictConstants.Parameters.RefreshToken] = refreshToken
        };

        // 刷新场景下 invalid_grant 表示刷新令牌失效，与密码错误区分开提示
        return RequestTokenAsync(
            parameters,
            ProjectDomainErrorCodes.InvalidRefreshToken,
            ProjectDomainErrorCodes.RefreshTokenFailed,
            cancellationToken);
    }

    /// <summary>
    /// 撤销指定令牌。
    /// </summary>
    /// <param name="token">待撤销的令牌值。</param>
    /// <param name="tokenTypeHint">令牌类型提示，如 refresh_token。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public virtual async Task RevokeAsync(
        string token,
        string tokenTypeHint,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigurationIsValid();

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(_options.RevocationEndpoint))
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.Token] = token,
                [OpenIddictConstants.Parameters.TokenTypeHint] = tokenTypeHint,
                [OpenIddictConstants.Parameters.ClientId] = _options.ClientId,
                [OpenIddictConstants.Parameters.ClientSecret] = _options.ClientSecret
            })
        };

        ApplyTenant(request);

        var response = await SendAsync(request, cancellationToken);

        // 撤销终结点成功时返回 200 空响应体，失败（如令牌已失效）按 OpenIddict 约定不做业务中断处理
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogWarning("Revoke token failed with status code {StatusCode}.", (int)response.StatusCode);
        }
    }

    /// <summary>
    /// 统一执行令牌请求：发送表单、解析响应、把失败响应翻译为业务异常。
    /// </summary>
    /// <param name="parameters">令牌终结点的表单参数。</param>
    /// <param name="invalidGrantErrorCode">AuthServer 返回 invalid_grant 时使用的错误码。</param>
    /// <param name="fallbackErrorCode">其他失败场景使用的错误码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>解析后的令牌响应。</returns>
    private async Task<TokenResponse> RequestTokenAsync(
        Dictionary<string, string> parameters,
        string invalidGrantErrorCode,
        string fallbackErrorCode,
        CancellationToken cancellationToken)
    {
        EnsureConfigurationIsValid();

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(_options.TokenEndpoint))
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        ApplyTenant(request);

        var response = await SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw BuildException(payload, invalidGrantErrorCode, fallbackErrorCode);
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(payload, JsonOptions);

        return tokenResponse ?? throw new BusinessException(ProjectDomainErrorCodes.TokenRequestFailed);
    }

    /// <summary>
    /// 使用命名 HttpClient 发送请求。
    /// </summary>
    /// <param name="request">已构造好的请求消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>原始 HTTP 响应。</returns>
    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 把当前租户透传给 AuthServer，保证多租户场景下找到正确的用户。
    /// </summary>
    /// <param name="request">待发送的请求消息。</param>
    private void ApplyTenant(HttpRequestMessage request)
    {
        // AuthServer 与业务服务使用同一套租户解析中间件，缺失该头会退化为 host 库查询
        if (_currentTenant.Id.HasValue)
        {
            request.Headers.TryAddWithoutValidation(AuthConsts.TenantKey, _currentTenant.Id.Value.ToString());
        }
    }

    /// <summary>
    /// 把 AuthServer 的错误响应翻译为带错误码与明细的业务异常。
    /// </summary>
    /// <param name="payload">AuthServer 返回的原始 JSON。</param>
    /// <param name="invalidGrantErrorCode">凭据/刷新令牌无效时使用的错误码。</param>
    /// <param name="fallbackErrorCode">其他错误使用的错误码。</param>
    /// <returns>待抛出的业务异常。</returns>
    private BusinessException BuildException(string payload, string invalidGrantErrorCode, string fallbackErrorCode)
    {
        TokenResponse? error = null;

        if (!payload.IsNullOrWhiteSpace())
        {
            try
            {
                error = JsonSerializer.Deserialize<TokenResponse>(payload, JsonOptions);
            }
            catch (JsonException)
            {
                // 非 JSON 响应（如网关错误页）时忽略解析结果，仅记录日志
                error = null;
            }
        }

        Logger.LogError(
            "Token request failed. Error: {Error}, Description: {Description}",
            error?.Error,
            error?.ErrorDescription);

        // 把 AuthServer 的原始错误描述放进 Details，便于排查（例如 client_id 未种子）
        var details = $"{(error?.Error ?? "unknown")}: {error?.ErrorDescription}";

        if (string.Equals(error?.Error, OpenIddictConstants.Errors.InvalidGrant, StringComparison.OrdinalIgnoreCase))
        {
            return new BusinessException(invalidGrantErrorCode, details: details);
        }

        return new BusinessException(fallbackErrorCode, details: details);
    }

    /// <summary>
    /// 拼接 AuthServer 终结点绝对地址。
    /// </summary>
    /// <param name="endpoint">以 / 开头的终结点路径。</param>
    /// <returns>完整的请求地址。</returns>
    private string BuildUrl(string endpoint)
    {
        EnsureConfigurationIsValid();

        return _options.Authority.TrimEnd('/') + "/" + endpoint.TrimStart('/');
    }

    /// <summary>
    /// 校验调用 AuthServer 所必需的配置是否齐全。
    /// </summary>
    /// <exception cref="BusinessException">缺少 Authority、ClientId 或 ClientSecret 时抛出。</exception>
    private void EnsureConfigurationIsValid()
    {
        if (_options.Authority.IsNullOrWhiteSpace() ||
            _options.ClientId.IsNullOrWhiteSpace() ||
            _options.ClientSecret.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ProjectDomainErrorCodes.AuthServerNotConfigured);
        }
    }
}
