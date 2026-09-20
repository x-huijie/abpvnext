using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门列表/树查询入参。
/// </summary>
public class GetDepartmentListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 名称或编码关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 指定上级部门，仅返回其直接下级；为 null 表示不按层级过滤。
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 按启用状态过滤；为 null 表示不过滤。
    /// </summary>
    public bool? IsActive { get; set; }
}
