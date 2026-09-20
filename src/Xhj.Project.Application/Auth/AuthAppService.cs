using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Auth;

/// <summary>
/// 认证入口：对外只需一次调用即可完成登录并取得令牌，内部由 <see cref="IAuthTokenClient"/> 与 AuthServer 交互。
/// </summary>
[AllowAnonymous]
public class AuthAppService : ProjectAppService, IAuthAppService
{
    private readonly IAuthTokenClient _authTokenClient;
    private readonly IdentityUserManager _identityUserManager;
    private readonly ICurrentTenant _currentTenant;

    public AuthAppService(
        IAuthTokenClient authTokenClient,
        IdentityUserManager identityUserManager,
        ICurrentTenant currentTenant)
    {
        _authTokenClient = authTokenClient;
        _identityUserManager = identityUserManager;
        _currentTenant = currentTenant;
    }

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

    [Authorize]
    public virtual Task LogoutAsync(LogoutInput input)
    {
        if (input.RefreshToken.IsNullOrWhiteSpace())
        {
            return Task.CompletedTask;
        }

        return _authTokenClient.RevokeAsync(input.RefreshToken!, AuthConsts.RefreshTokenTypeHint);
    }

    /// <summary>
    /// 登录后补充用户信息，便于前端直接展示，不作为登录成功与否的依据。
    /// </summary>
    private async Task<IdentityUser?> FindUserAsync(string userNameOrEmailAddress)
    {
        var user = await _identityUserManager.FindByNameAsync(userNameOrEmailAddress);

        return user ?? await _identityUserManager.FindByEmailAsync(userNameOrEmailAddress);
    }

    private AuthTokenDto BuildTokenDto(TokenResponse token, IdentityUser? user)
    {
        var expiresIn = token.ExpiresIn ?? 0;

        return new AuthTokenDto
        {
            TokenType = token.TokenType ?? AuthConsts.BearerTokenType,
            AccessToken = token.AccessToken!,
            RefreshToken = token.RefreshToken,
            ExpiresIn = expiresIn,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn),
            UserId = user?.Id,
            UserName = user?.UserName,
            Email = user?.Email,
            TenantId = _currentTenant.Id
        };
    }
}
