using System;
using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单新增/编辑入参。
/// </summary>
/// <remarks>
/// 上级菜单变更不在本模型中，需调用 <c>PUT /api/app/menu/{id}/move</c>。
/// </remarks>
public class CreateUpdateMenuDto
{
    /// <summary>
    /// 菜单标识，必填且租户内唯一，长度见 <see cref="MenuConsts.MaxNameLength"/>。
    /// </summary>
    [Required]
    [MaxLength(MenuConsts.MaxNameLength)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 显示名，必填，长度见 <see cref="MenuConsts.MaxDisplayNameLength"/>。
    /// </summary>
    [Required]
    [MaxLength(MenuConsts.MaxDisplayNameLength)]
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// 上级菜单 Id，可为空表示顶级菜单。
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 前端路由地址，可为空（纯分组节点无路由）。
    /// </summary>
    [MaxLength(MenuConsts.MaxPathLength)]
    public string? Path { get; set; }

    /// <summary>
    /// 前端组件路径，可为空。
    /// </summary>
    [MaxLength(MenuConsts.MaxComponentLength)]
    public string? Component { get; set; }

    /// <summary>
    /// 菜单图标，可为空。
    /// </summary>
    [MaxLength(MenuConsts.MaxIconLength)]
    public string? Icon { get; set; }

    /// <summary>
    /// 访问该菜单所需的权限名；为空表示登录即可见。
    /// </summary>
    [MaxLength(MenuConsts.MaxPermissionLength)]
    public string? Permission { get; set; }

    /// <summary>
    /// 排序号，值越小越靠前，默认 0。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用，默认 true。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否在侧边栏显示，默认 true；false 可作为隐藏路由。
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// 备注，可为空。
    /// </summary>
    [MaxLength(MenuConsts.MaxRemarkLength)]
    public string? Remark { get; set; }
}
