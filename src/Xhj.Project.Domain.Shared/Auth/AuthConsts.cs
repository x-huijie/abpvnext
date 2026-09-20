namespace Xhj.Project.Auth;

/// <summary>
/// 认证/Token 相关的常量与协议名，供各层共享。
/// </summary>
public static class AuthConsts
{
    public const string PasswordGrantType = "password";

    public const string RefreshTokenGrantType = "refresh_token";

    public const string OfflineAccessScope = "offline_access";

    public const string RefreshTokenTypeHint = "refresh_token";

    public const string AccessTokenTypeHint = "access_token";

    public const string BearerTokenType = "Bearer";

    /// <summary>
    /// ABP 默认的租户解析请求头/查询参数名，调用 AuthServer 时需要透传。
    /// </summary>
    public const string TenantKey = "__tenant";

    public const int MaxUserNameOrEmailAddressLength = 255;

    public const int MaxPasswordLength = 128;

    public const int MaxRefreshTokenLength = 2048;
}
