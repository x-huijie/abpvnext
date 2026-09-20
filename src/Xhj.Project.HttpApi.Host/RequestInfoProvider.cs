using System;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;
using Xhj.Project.OperationLogs;

namespace Xhj.Project;

/// <summary>
/// 请求环境信息提供者的 ASP.NET Core 实现（适配器）。
/// </summary>
/// <remarks>
/// Application 层不依赖 HttpContext，因此把 IP/UserAgent 的读取实现放在宿主层；
/// 后台任务等无 HTTP 的宿主可以不注册该实现，操作日志会自动跳过客户端信息。
/// </remarks>
public class RequestInfoProvider : IRequestInfoProvider, ITransientDependency
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造请求环境信息提供者。
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器，由 Host 模块注册。</param>
    public RequestInfoProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 取客户端 IP 地址；取不到时返回 null。
    /// </summary>
    /// <returns>IP 地址字符串。</returns>
    public virtual string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return null;
        }

        // 反向代理场景下优先取 X-Forwarded-For 中的原始 IP
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();

        if (!forwardedFor.IsNullOrEmpty())
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection?.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// 取客户端 UserAgent；取不到时返回 null。
    /// </summary>
    /// <returns>UserAgent 字符串。</returns>
    public virtual string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}
