namespace Xhj.Project.Auth;

/// <summary>
/// 调用 AuthServer 令牌终结点所需的配置（由 HttpApi.Host 从 AuthServer 配置节绑定）。
/// </summary>
public class AuthTokenOptions
{
    /// <summary>
    /// AuthServer 根地址，例如 https://localhost:44311
    /// </summary>
    public string Authority { get; set; } = default!;

    public string TokenEndpoint { get; set; } = "/connect/token";

    public string RevocationEndpoint { get; set; } = "/connect/revocation";

    public string ClientId { get; set; } = default!;

    public string ClientSecret { get; set; } = default!;

    /// <summary>
    /// 申请的 scope，必须包含 offline_access 才能拿到 RefreshToken。
    /// </summary>
    public string Scope { get; set; } = "Project offline_access";

    public int TimeoutSeconds { get; set; } = 30;
}
