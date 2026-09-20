using System;

namespace Xhj.Project.Auth;

/// <summary>
/// 登录/刷新统一返回的令牌结果。
/// </summary>
public class AuthTokenDto
{
    public string TokenType { get; set; } = AuthConsts.BearerTokenType;

    public string AccessToken { get; set; } = default!;

    public string? RefreshToken { get; set; }

    /// <summary>
    /// 访问令牌有效期（秒）。
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 访问令牌过期时间（UTC）。
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public Guid? TenantId { get; set; }
}
