using System.Threading;
using System.Threading.Tasks;

namespace Xhj.Project.Auth;

/// <summary>
/// 令牌网关门面：屏蔽对 AuthServer /connect 系列终结点的 HTTP 协议细节。
/// </summary>
/// <remarks>
/// 属于端口-适配器中的"端口"，实现放在 <see cref="AuthTokenClient"/>，
/// 便于将来替换协议（如改用 IdentityServer）或为单元测试提供替身。
/// </remarks>
public interface IAuthTokenClient
{
    /// <summary>
    /// 使用用户名（或邮箱）与密码，按 password 模式申请令牌。
    /// </summary>
    /// <param name="userNameOrEmailAddress">用户名或邮箱地址。</param>
    /// <param name="password">明文密码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>令牌终结点的原始响应。</returns>
    Task<TokenResponse> RequestPasswordTokenAsync(
        string userNameOrEmailAddress,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用 RefreshToken 换取新的访问令牌。
    /// </summary>
    /// <param name="refreshToken">上一次下发的刷新令牌。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>令牌终结点的原始响应。</returns>
    Task<TokenResponse> RequestRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 撤销指定令牌（通常是 RefreshToken）。
    /// </summary>
    /// <param name="token">待撤销的令牌值。</param>
    /// <param name="tokenTypeHint">令牌类型提示，如 refresh_token。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task RevokeAsync(
        string token,
        string tokenTypeHint,
        CancellationToken cancellationToken = default);
}
