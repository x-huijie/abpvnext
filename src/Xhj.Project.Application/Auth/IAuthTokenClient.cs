using System.Threading;
using System.Threading.Tasks;

namespace Xhj.Project.Auth;

/// <summary>
/// 令牌网关门面：屏蔽对 AuthServer /connect 系列终结点的 HTTP 细节。
/// </summary>
public interface IAuthTokenClient
{
    Task<TokenResponse> RequestPasswordTokenAsync(
        string userNameOrEmailAddress,
        string password,
        CancellationToken cancellationToken = default);

    Task<TokenResponse> RequestRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string token,
        string tokenTypeHint,
        CancellationToken cancellationToken = default);
}
