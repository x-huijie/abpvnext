using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Auth;

/// <summary>
/// 登出入参：可选地撤销一个 RefreshToken。
/// </summary>
/// <remarks>
/// 由 <c>POST /api/app/auth/logout</c> 使用，需要携带有效的 Bearer 令牌。
/// </remarks>
public class LogoutInput
{
    /// <summary>
    /// 需要撤销的 RefreshToken，可为空。
    /// </summary>
    /// <remarks>
    /// 为空时服务端不做任何撤销动作（访问令牌本身无法撤销，只能等其过期）。
    /// </remarks>
    [MaxLength(AuthConsts.MaxRefreshTokenLength)]
    public string? RefreshToken { get; set; }
}
