using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Xhj.Project.Auth;

/// <summary>
/// 认证应用服务：对外提供"登录即换取令牌"的统一入口。
/// </summary>
/// <remarks>
/// 设计目的：客户端只需一次调用完成登录，不必先登录再单独请求 /connect/token；
/// AuthServer 的协议细节（grant_type、client_secret、__tenant）全部封装在服务端。
/// </remarks>
public interface IAuthAppService : IApplicationService
{
    /// <summary>
    /// 登录：校验凭据并向 AuthServer 申请 password 模式令牌。
    /// </summary>
    /// <param name="input">用户名（或邮箱）与密码。</param>
    /// <returns>访问令牌、刷新令牌及用户信息。</returns>
    Task<AuthTokenDto> LoginAsync(LoginInput input);

    /// <summary>
    /// 使用 RefreshToken 换取新的访问令牌。
    /// </summary>
    /// <param name="input">上一次返回的 RefreshToken。</param>
    /// <returns>新的访问令牌与刷新令牌。</returns>
    Task<AuthTokenDto> RefreshTokenAsync(RefreshTokenInput input);

    /// <summary>
    /// 登出：撤销传入的 RefreshToken（若提供）。
    /// </summary>
    /// <param name="input">待撤销的 RefreshToken，可为空。</param>
    Task LogoutAsync(LogoutInput input);
}
