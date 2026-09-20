using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Xhj.Project.Auth;

/// <summary>
/// 认证应用服务：登录并换取令牌的统一入口。
/// </summary>
public interface IAuthAppService : IApplicationService
{
    /// <summary>
    /// 登录，内部调用 AuthServer 的 password 模式换取令牌。
    /// </summary>
    Task<AuthTokenDto> LoginAsync(LoginInput input);

    /// <summary>
    /// 使用 RefreshToken 换取新的访问令牌。
    /// </summary>
    Task<AuthTokenDto> RefreshTokenAsync(RefreshTokenInput input);

    /// <summary>
    /// 登出，撤销 RefreshToken（若提供）。
    /// </summary>
    Task LogoutAsync(LogoutInput input);
}
