using System;

namespace Xhj.Project.Auth;

/// <summary>
/// 登录与刷新令牌共用的返回结果。
/// </summary>
/// <remarks>
/// 客户端拿到后直接拼 <c>Authorization: {TokenType} {AccessToken}</c> 即可调用业务接口。
/// </remarks>
public class AuthTokenDto
{
    /// <summary>
    /// 令牌类型，默认 <see cref="AuthConsts.BearerTokenType"/>。
    /// </summary>
    public string TokenType { get; set; } = AuthConsts.BearerTokenType;

    /// <summary>
    /// 访问令牌（JWT），调用业务接口时携带。
    /// </summary>
    public string AccessToken { get; set; } = default!;

    /// <summary>
    /// 刷新令牌，用于换取新的访问令牌；未申请 offline_access 时为 null。
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 访问令牌有效期（秒）。
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 访问令牌过期时间（UTC），前端展示或自动刷新时需自行换算时区。
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 登录用户 Id；刷新令牌场景下无法得知账号，为 null。
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 登录用户名；刷新令牌场景下为 null。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 登录用户邮箱；刷新令牌场景下为 null。
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 当前租户 Id；host 登录时为 null。
    /// </summary>
    public Guid? TenantId { get; set; }
}
