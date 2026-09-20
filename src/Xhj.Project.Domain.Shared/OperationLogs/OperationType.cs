namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志的操作类型。
/// </summary>
/// <remarks>
/// 与 ABP 审计日志的区别：审计日志自动记录每一次接口调用（含参数与耗时），
/// 操作日志由业务代码手动埋点，只记录"重要业务动作"及其业务语义。
/// </remarks>
public enum OperationType
{
    /// <summary>
    /// 新增数据。
    /// </summary>
    Create = 1,

    /// <summary>
    /// 修改数据。
    /// </summary>
    Update = 2,

    /// <summary>
    /// 删除数据。
    /// </summary>
    Delete = 3,

    /// <summary>
    /// 导出数据。
    /// </summary>
    Export = 4,

    /// <summary>
    /// 导入数据。
    /// </summary>
    Import = 5,

    /// <summary>
    /// 审核/审批动作。
    /// </summary>
    Approve = 6,

    /// <summary>
    /// 登录/登出等账号动作。
    /// </summary>
    Account = 7,

    /// <summary>
    /// 其他业务动作。
    /// </summary>
    Other = 99
}
