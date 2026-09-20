using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单列表/树查询入参。
/// </summary>
public class GetMenuListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 菜单标识或显示名关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 上级菜单 Id，仅返回其直接下级；为 null 表示不按层级过滤。
    /// </summary>
    public System.Guid? ParentId { get; set; }

    /// <summary>
    /// 按启用状态过滤；为 null 表示不过滤。
    /// </summary>
    public bool? IsEnabled { get; set; }
}
