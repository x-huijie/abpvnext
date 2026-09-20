using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志列表查询入参。
/// </summary>
public class GetOperationLogListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 业务模块名（精确匹配），为空表示不过滤。
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 操作类型，为 null 表示不过滤。
    /// </summary>
    public OperationType? OperationType { get; set; }

    /// <summary>
    /// 操作人 Id，为 null 表示不过滤。
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 是否只看失败的操作，为 null 表示不过滤。
    /// </summary>
    public bool? IsSuccess { get; set; }

    /// <summary>
    /// 关键字：匹配操作名称或描述，为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 起始时间（含），按创建时间过滤。
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间（含），按创建时间过滤。
    /// </summary>
    public DateTime? EndTime { get; set; }
}
