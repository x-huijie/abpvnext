using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志输出 DTO。
/// </summary>
public class OperationLogDto : CreationAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 业务模块名。
    /// </summary>
    public string Module { get; set; } = default!;

    /// <summary>
    /// 操作名称。
    /// </summary>
    public string Operation { get; set; } = default!;

    /// <summary>
    /// 操作类型。
    /// </summary>
    public OperationType OperationType { get; set; }

    /// <summary>
    /// 操作内容描述，可为空。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 关联实体类型，可为空。
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// 关联实体 Id，可为空。
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// 操作人 Id，可为空。
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作人用户名，可为空。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 客户端 IP，可为空。
    /// </summary>
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// 操作耗时（毫秒），可为空。
    /// </summary>
    public int? ExecutionDuration { get; set; }

    /// <summary>
    /// 操作是否成功。
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 失败原因，成功时为空。
    /// </summary>
    public string? ErrorMessage { get; set; }
}
