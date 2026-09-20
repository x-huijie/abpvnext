using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Auth;

/// <summary>
/// 认证入口：对外只需一次调用即可完成登录并取得令牌。
/// </summary>
/// <remarks>
/// 编排职责：调用 <see cref="IAuthTokenClient"/> 换取令牌 → 按租户与账号补充用户信息 → 组装 <see cref="AuthTokenDto"/>。
/// 登录与刷新允许匿名访问；登出需要先通过鉴权。
/// </remarks>
[AllowAnonymous]
public class AuthAppService : ProjectAppService, IAuthAppService
{
    private readonly IAuthTokenClient _authTokenClient;
    private readonly IdentityUserManager _identityUserManager;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// 构造认证应用服务。
    /// </summary>
    /// <param name="authTokenClient">AuthServer 令牌网关。</param>
    /// <param name="identityUserManager">用于登录后补充用户信息。</param>
    /// <param name="currentTenant">当前租户，用于回填租户 Id。</param>
    public AuthAppService(
        IAuthTokenClient authTokenClient,
        IdentityUserManager identityUserManager,
        ICurrentTenant currentTenant)
    {
        _authTokenClient = authTokenClient;
        _identityUserManager = identityUserManager;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 登录并换取令牌。
    /// </summary>
    /// <param name="input">用户名（或邮箱）与密码。</param>
    /// <returns>令牌与用户信息。</returns>
    /// <exception cref="BusinessException">凭据错误或 AuthServer 不可用时抛出。</exception>
    public virtual async Task<AuthTokenDto> LoginAsync(LoginInput input)
    {
        var token = await _authTokenClient.RequestPasswordTokenAsync(
            input.UserNameOrEmailAddress,
            input.Password);

        if (token.AccessToken.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ProjectDomainErrorCodes.TokenRequestFailed);
        }

        var user = await FindUserAsync(input.UserNameOrEmailAddress);

        return BuildTokenDto(token, user);
    }

    /// <summary>
    /// 使用 RefreshToken 换取新的访问令牌。
    /// </summary>
    /// <param name="input">RefreshToken。</param>
    /// <returns>新的令牌。</returns>
    /// <exception cref="BusinessException">刷新令牌无效或已过期时抛出。</exception>
    public virtual async Task<AuthTokenDto> RefreshTokenAsync(RefreshTokenInput input)
    {
        var token = await _authTokenClient.RequestRefreshTokenAsync(input.RefreshToken);

        if (token.AccessToken.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ProjectDomainErrorCodes.RefreshTokenFailed);
        }

        // 刷新令牌时无法得知账号，仅从结构化的令牌信息中回填租户
        return BuildTokenDto(token, null);
    }

    /// <summary>
    /// 登出：撤销传入的 RefreshToken。
    /// </summary>
    /// <param name="input">待撤销的 RefreshToken，可为空。</param>
    [Authorize]
    public virtual Task LogoutAsync(LogoutInput input)
    {
        // 访问令牌无法撤销，只能等其自然过期，因此没有 RefreshToken 时无需处理
        if (input.RefreshToken.IsNullOrWhiteSpace())
        {
            return Task.CompletedTask;
        }

        return _authTokenClient.RevokeAsync(input.RefreshToken!, AuthConsts.RefreshTokenTypeHint);
    }

    /// <summary>
    /// 登录后补充用户信息，便于前端直接展示，不作为登录成功与否的依据。
    /// </summary>
    /// <param name="userNameOrEmailAddress">用户名或邮箱地址。</param>
    /// <returns>命中的用户；未找到时返回 null（登录仍视为成功）。</returns>
    private async Task<IdentityUser?> FindUserAsync(string userNameOrEmailAddress)
    {
        var user = await _identityUserManager.FindByNameAsync(userNameOrEmailAddress);

        return user ?? await _identityUserManager.FindByEmailAsync(userNameOrEmailAddress);
    }

    /// <summary>
    /// 由令牌响应与用户信息组装返回结果。
    /// </summary>
    /// <param name="token">AuthServer 返回的原始令牌响应。</param>
    /// <param name="user">命中的用户，刷新场景下为 null。</param>
    /// <returns>统一令牌结果。</returns>
    private AuthTokenDto BuildTokenDto(TokenResponse token, IdentityUser? user)
    {
        var expiresIn = token.ExpiresIn ?? 0;

        return new AuthTokenDto
        {
            TokenType = token.TokenType ?? AuthConsts.BearerTokenType,
            AccessToken = token.AccessToken!,
            RefreshToken = token.RefreshToken,
            ExpiresIn = expiresIn,
            // 统一输出 UTC，由前端自行换算本地时区
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn),
            UserId = user?.Id,
            UserName = user?.UserName,
            Email = user?.Email,
            TenantId = _currentTenant.Id
        };
    }
}
