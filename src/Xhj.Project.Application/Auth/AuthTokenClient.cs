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
public class AuthTokenClient : IAuthTokenClient, ITransientDependency
{
    /// <summary>
    /// 由 HttpApi.Host 注册的命名 HttpClient。
    /// </summary>
    public const string HttpClientName = "ProjectAuthTokenClient";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthTokenOptions _options;
    private readonly ICurrentTenant _currentTenant;

    public ILogger<AuthTokenClient> Logger { get; set; }

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

        return RequestTokenAsync(
            parameters,
            ProjectDomainErrorCodes.InvalidUserNameOrPassword,
            ProjectDomainErrorCodes.TokenRequestFailed,
            cancellationToken);
    }

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

        return RequestTokenAsync(
            parameters,
            ProjectDomainErrorCodes.InvalidRefreshToken,
            ProjectDomainErrorCodes.RefreshTokenFailed,
            cancellationToken);
    }

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

    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 把当前租户透传给 AuthServer，保证多租户场景下找到正确的用户。
    /// </summary>
    private void ApplyTenant(HttpRequestMessage request)
    {
        if (_currentTenant.Id.HasValue)
        {
            request.Headers.TryAddWithoutValidation(AuthConsts.TenantKey, _currentTenant.Id.Value.ToString());
        }
    }

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

    private string BuildUrl(string endpoint)
    {
        EnsureConfigurationIsValid();

        return _options.Authority.TrimEnd('/') + "/" + endpoint.TrimStart('/');
    }

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
