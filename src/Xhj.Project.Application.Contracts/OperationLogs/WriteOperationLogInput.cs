using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 手动埋点写入操作日志的入参。
/// </summary>
public class WriteOperationLogInput
{
    /// <summary>
    /// 业务模块名，必填，如 "Departments"。
    /// </summary>
    [Required]
    [MaxLength(OperationLogConsts.MaxModuleLength)]
    public string Module { get; set; } = default!;

    /// <summary>
    /// 操作名称，必填，如 "删除部门"。
    /// </summary>
    [Required]
    [MaxLength(OperationLogConsts.MaxOperationLength)]
    public string Operation { get; set; } = default!;

    /// <summary>
    /// 操作类型，必填。
    /// </summary>
    [Required]
    public OperationType OperationType { get; set; }

    /// <summary>
    /// 操作内容描述，建议带上关键业务信息（编码、名称、变更前后）。
    /// </summary>
    [MaxLength(OperationLogConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    /// <summary>
    /// 关联实体类型全名，可为空。
    /// </summary>
    [MaxLength(OperationLogConsts.MaxEntityTypeLength)]
    public string? EntityType { get; set; }

    /// <summary>
    /// 关联实体 Id，可为空。
    /// </summary>
    [MaxLength(OperationLogConsts.MaxEntityIdLength)]
    public string? EntityId { get; set; }

    /// <summary>
    /// 操作耗时（毫秒），可为空。
    /// </summary>
    public int? ExecutionDuration { get; set; }

    /// <summary>
    /// 操作是否成功，默认 true。
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// 失败原因，成功时可为空。
    /// </summary>
    [MaxLength(OperationLogConsts.MaxErrorMessageLength)]
    public string? ErrorMessage { get; set; }
}
