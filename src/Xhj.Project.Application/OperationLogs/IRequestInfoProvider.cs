namespace Xhj.Project.OperationLogs;

/// <summary>
/// 请求环境信息提供者（端口）：屏蔽对 HttpContext 的直接依赖。
/// </summary>
/// <remarks>
/// Application 层不引用 ASP.NET Core，因此把 IP/UserAgent 的获取抽象成接口，
/// 由 HttpApi.Host 提供实现（适配器），后台任务等无 HTTP 场景下可不注册。
/// </remarks>
public interface IRequestInfoProvider
{
    /// <summary>
    /// 取客户端 IP 地址；取不到时返回 null。
    /// </summary>
    /// <returns>IP 地址字符串。</returns>
    string? GetClientIpAddress();

    /// <summary>
    /// 取客户端 UserAgent；取不到时返回 null。
    /// </summary>
    /// <returns>UserAgent 字符串。</returns>
    string? GetUserAgent();
}
