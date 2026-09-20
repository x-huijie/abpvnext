using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Auth;

/// <summary>
/// 登录入参：用户名（或邮箱）+ 密码。
/// </summary>
public class LoginInput
{
    [Required]
    [MaxLength(AuthConsts.MaxUserNameOrEmailAddressLength)]
    public string UserNameOrEmailAddress { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    [MaxLength(AuthConsts.MaxPasswordLength)]
    public string Password { get; set; } = default!;
}
