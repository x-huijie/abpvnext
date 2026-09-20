using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Auth;

/// <summary>
/// 刷新令牌入参。
/// </summary>
public class RefreshTokenInput
{
    [Required]
    [MaxLength(AuthConsts.MaxRefreshTokenLength)]
    public string RefreshToken { get; set; } = default!;
}
