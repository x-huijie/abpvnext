using System.Text.Json.Serialization;

namespace Xhj.Project.Auth;

/// <summary>
/// OpenIddict 令牌终结点的原始响应模型。
/// </summary>
/// <remarks>
/// 成功响应与错误响应共用同一模型：成功时带 access_token，失败时带 error/error_description。
/// 字段名遵循 OAuth2 的 snake_case 约定，因此逐个标注 JsonPropertyName。
/// </remarks>
public class TokenResponse
{
    /// <summary>
    /// 访问令牌（JWT），成功时返回。
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// 令牌类型，通常为 Bearer。
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    /// <summary>
    /// 访问令牌有效期（秒）。
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// 刷新令牌，申请了 offline_access 时返回。
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 实际授予的 scope 列表（空格分隔）。
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// 错误码，如 invalid_grant、invalid_client。
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// 错误描述，用于排查（如 The specified 'client_id' is invalid）。
    /// </summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}
