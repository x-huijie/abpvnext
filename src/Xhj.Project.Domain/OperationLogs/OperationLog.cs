using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志聚合根：记录重要业务动作的<b>业务语义</b>，与 ABP 审计日志互补。
/// </summary>
/// <remarks>
/// 审计日志（AbpAuditLogs）由框架自动记录每一次接口调用；
/// 本实体由业务代码通过 <c>IOperationLogWriter</c> 手动埋点，只记录需要追溯的关键操作。
/// 记录失败不能影响主流程，因此写入端会吞掉异常并只记日志。
/// </remarks>
public class OperationLog : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 业务模块名，如 "Departments""Files"。
    /// </summary>
    public virtual string Module { get; protected set; } = default!;

    /// <summary>
    /// 操作名称，如 "删除部门"。
    /// </summary>
    public virtual string Operation { get; protected set; } = default!;

    /// <summary>
    /// 操作类型。
    /// </summary>
    public virtual OperationType OperationType { get; protected set; }

    /// <summary>
    /// 操作内容描述，建议包含关键业务信息（如编码、名称、变更前后）。
    /// </summary>
    public virtual string? Description { get; protected set; }

    /// <summary>
    /// 关联的实体类型全名，可为空。
    /// </summary>
    public virtual string? EntityType { get; protected set; }

    /// <summary>
    /// 关联的实体 Id（字符串存储，兼容非 Guid 主键），可为空。
    /// </summary>
    public virtual string? EntityId { get; protected set; }

    /// <summary>
    /// 操作人 Id，可为空（系统自动任务）。
    /// </summary>
    public virtual Guid? UserId { get; protected set; }

    /// <summary>
    /// 操作人用户名（冗余存储，避免联表）。
    /// </summary>
    public virtual string? UserName { get; protected set; }

    /// <summary>
    /// 客户端 IP，可为空。
    /// </summary>
    public virtual string? ClientIpAddress { get; protected set; }

    /// <summary>
    /// 客户端 UserAgent，可为空。
    /// </summary>
    public virtual string? UserAgent { get; protected set; }

    /// <summary>
    /// 操作耗时（毫秒），可为空。
    /// </summary>
    public virtual int? ExecutionDuration { get; protected set; }

    /// <summary>
    /// 操作是否成功；失败时看 <see cref="ErrorMessage"/>。
    /// </summary>
    public virtual bool IsSuccess { get; protected set; }

    /// <summary>
    /// 失败原因，成功时为空。
    /// </summary>
    public virtual string? ErrorMessage { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected OperationLog()
    {
        Module = string.Empty;
        Operation = string.Empty;
    }

    /// <summary>
    /// 创建一条操作日志。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="module">业务模块名。</param>
    /// <param name="operation">操作名称。</param>
    /// <param name="operationType">操作类型。</param>
    /// <param name="description">操作描述，可为空。</param>
    /// <param name="entityType">关联实体类型，可为空。</param>
    /// <param name="entityId">关联实体 Id，可为空。</param>
    public OperationLog(
        Guid id,
        string module,
        string operation,
        OperationType operationType,
        string? description = null,
        string? entityType = null,
        string? entityId = null)
        : base(id)
    {
        Module = module;
        Operation = operation;
        OperationType = operationType;
        Description = description;
        EntityType = entityType;
        EntityId = entityId;

        // 默认视为成功，失败时由 MarkAsFailed 覆盖
        IsSuccess = true;
    }

    /// <summary>
    /// 写入操作人信息。
    /// </summary>
    /// <param name="userId">操作人 Id，可为空。</param>
    /// <param name="userName">操作人用户名，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual OperationLog SetUser(Guid? userId, string? userName)
    {
        UserId = userId;
        UserName = userName;
        return this;
    }

    /// <summary>
    /// 写入客户端环境信息（IP 与 UserAgent）。
    /// </summary>
    /// <param name="clientIpAddress">客户端 IP，可为空。</param>
    /// <param name="userAgent">UserAgent，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual OperationLog SetClientInfo(string? clientIpAddress, string? userAgent)
    {
        ClientIpAddress = clientIpAddress;
        UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// 写入操作耗时。
    /// </summary>
    /// <param name="duration">毫秒数。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual OperationLog SetExecutionDuration(int? duration)
    {
        ExecutionDuration = duration;
        return this;
    }

    /// <summary>
    /// 标记该操作失败并记录原因。
    /// </summary>
    /// <param name="errorMessage">失败原因。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual OperationLog MarkAsFailed(string? errorMessage)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
        return this;
    }
}
