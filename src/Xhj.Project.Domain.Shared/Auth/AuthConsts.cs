namespace Xhj.Project.Auth;

/// <summary>
/// 认证/Token 相关的常量与协议名，供 Domain.Shared 及其上层共享。
/// </summary>
/// <remarks>
/// 这里只放跨层共享的约束与协议约定，不放任何业务行为。
/// </remarks>
public static class AuthConsts
{
    /// <summary>
    /// OAuth2 密码模式（password grant）的 grant_type 取值。
    /// </summary>
    public const string PasswordGrantType = "password";

    /// <summary>
    /// OAuth2 刷新令牌模式（refresh_token grant）的 grant_type 取值。
    /// </summary>
    public const string RefreshTokenGrantType = "refresh_token";

    /// <summary>
    /// OpenIddict 中用于签发 RefreshToken 的必要 scope。
    /// </summary>
    public const string OfflineAccessScope = "offline_access";

    /// <summary>
    /// 撤销令牌时 token_type_hint 的"刷新令牌"取值。
    /// </summary>
    public const string RefreshTokenTypeHint = "refresh_token";

    /// <summary>
    /// 撤销令牌时 token_type_hint 的"访问令牌"取值（当前仅用于扩展）。
    /// </summary>
    public const string AccessTokenTypeHint = "access_token";

    /// <summary>
    /// 令牌类型的默认值，用于前端拼接 Authorization 头。
    /// </summary>
    public const string BearerTokenType = "Bearer";

    /// <summary>
    /// ABP 默认的租户解析请求头/查询参数名（__tenant）。
    /// </summary>
    /// <remarks>
    /// 调用 AuthServer 时必须透传该头，否则多租户场景下会去 host 库里找用户。
    /// </remarks>
    public const string TenantKey = "__tenant";

    /// <summary>
    /// 用户名/邮箱的最大长度。
    /// </summary>
    public const int MaxUserNameOrEmailAddressLength = 255;

    /// <summary>
    /// 密码的最大长度。
    /// </summary>
    public const int MaxPasswordLength = 128;

    /// <summary>
    /// 刷新令牌的最大长度。
    /// </summary>
    public const int MaxRefreshTokenLength = 2048;
}
