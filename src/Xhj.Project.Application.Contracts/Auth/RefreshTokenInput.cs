using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Auth;

/// <summary>
/// 刷新令牌入参：使用上一次登录返回的 RefreshToken 换取新的访问令牌。
/// </summary>
/// <remarks>
/// 由 <c>POST /api/app/auth/refresh-token</c> 使用，匿名可访问。
/// </remarks>
public class RefreshTokenInput
{
    /// <summary>
    /// 登录或上次刷新时返回的 RefreshToken，必填。
    /// </summary>
    /// <remarks>
    /// 只有申请了 <see cref="AuthConsts.OfflineAccessScope"/> 时才会下发该令牌。
    /// </remarks>
    [Required]
    [MaxLength(AuthConsts.MaxRefreshTokenLength)]
    public string RefreshToken { get; set; } = default!;
}
