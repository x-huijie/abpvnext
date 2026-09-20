namespace Xhj.Project.Auth;

/// <summary>
/// 调用 AuthServer 令牌终结点所需的配置。
/// </summary>
/// <remarks>
/// 由 HttpApi.Host 在 <c>ProjectHttpApiHostModule.ConfigureAuthToken</c> 中从 <c>AuthServer</c> 配置节绑定。
/// </remarks>
public class AuthTokenOptions
{
    /// <summary>
    /// AuthServer 根地址，例如 <c>https://localhost:44311</c>。
    /// </summary>
    public string Authority { get; set; } = default!;

    /// <summary>
    /// 令牌终结点路径，默认 <c>/connect/token</c>（OpenIddict 约定）。
    /// </summary>
    public string TokenEndpoint { get; set; } = "/connect/token";

    /// <summary>
    /// 撤销终结点路径，默认 <c>/connect/revocation</c>（OpenIddict 约定）。
    /// </summary>
    public string RevocationEndpoint { get; set; } = "/connect/revocation";

    /// <summary>
    /// OpenIddict 客户端 Id，需与 AuthServer 种子出来的应用一致（本项目为 Project_App）。
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// OpenIddict 客户端密钥，需与 AuthServer 种子出来的应用一致。
    /// </summary>
    public string ClientSecret { get; set; } = default!;

    /// <summary>
    /// 申请的 scope，必须包含 <see cref="AuthConsts.OfflineAccessScope"/> 才能拿到 RefreshToken。
    /// </summary>
    public string Scope { get; set; } = "Project offline_access";

    /// <summary>
    /// HTTP 调用超时时间（秒），在 HttpClient 注册时使用。
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
