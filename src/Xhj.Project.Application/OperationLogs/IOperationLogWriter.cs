using System.Threading.Tasks;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志写入器：业务代码在关键动作处手动埋点调用。
/// </summary>
/// <remarks>
/// 与审计日志的区别：审计日志由 ABP 自动记录每一次接口调用；
/// 这里的写入是<b>显式</b>的，只用于需要追溯的重要业务操作（新增订单、删除数据、审核等）。
/// 写入失败不会向调用方抛出异常，避免日志影响主流程。
/// </remarks>
public interface IOperationLogWriter
{
    /// <summary>
    /// 记录一条操作日志。
    /// </summary>
    /// <param name="input">操作信息。</param>
    /// <returns>异步任务。</returns>
    Task WriteAsync(WriteOperationLogInput input);
}
