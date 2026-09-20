using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Auth;

/// <summary>
/// 登录入参：用户名（或邮箱）+ 密码。
/// </summary>
/// <remarks>
/// 由 <c>POST /api/app/auth/login</c> 使用，匿名可访问；
/// 服务端内部再向 AuthServer 发起 password 模式换取令牌。
/// </remarks>
public class LoginInput
{
    /// <summary>
    /// 用户名或邮箱地址，必填，最大长度见 <see cref="AuthConsts.MaxUserNameOrEmailAddressLength"/>。
    /// </summary>
    [Required]
    [MaxLength(AuthConsts.MaxUserNameOrEmailAddressLength)]
    public string UserNameOrEmailAddress { get; set; } = default!;

    /// <summary>
    /// 明文密码，必填，最大长度见 <see cref="AuthConsts.MaxPasswordLength"/>。
    /// </summary>
    /// <remarks>
    /// 仅用于向 AuthServer 提交凭据，本服务不做持久化，也不会记录日志。
    /// </remarks>
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(AuthConsts.MaxPasswordLength)]
    public string Password { get; set; } = default!;
}
