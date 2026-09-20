using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限规则列表查询入参。
/// </summary>
public class GetDataPermissionRuleListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 按角色过滤；为 null 表示不过滤。
    /// </summary>
    public Guid? RoleId { get; set; }

    /// <summary>
    /// 按资源标识过滤（精确匹配）；为空表示不过滤。
    /// </summary>
    public string? ResourceKey { get; set; }

    /// <summary>
    /// 按启用状态过滤；为 null 表示不过滤。
    /// </summary>
    public bool? IsEnabled { get; set; }
}
