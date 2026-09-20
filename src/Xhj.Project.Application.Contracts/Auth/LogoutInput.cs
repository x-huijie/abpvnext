using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Auth;

/// <summary>
/// 登出入参：撤销 RefreshToken（可选）。
/// </summary>
public class LogoutInput
{
    [MaxLength(AuthConsts.MaxRefreshTokenLength)]
    public string? RefreshToken { get; set; }
}
