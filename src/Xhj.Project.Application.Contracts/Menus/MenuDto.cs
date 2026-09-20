using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单输出 DTO，用于列表与详情返回。
/// </summary>
public class MenuDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 菜单标识，租户内唯一。
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 菜单显示名。
    /// </summary>
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// 上级菜单 Id；为 null 表示顶级菜单。
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 前端路由地址，可为空。
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 前端组件路径，可为空。
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 菜单图标，可为空。
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 访问该菜单所需的权限名；为空表示登录即可见。
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 排序号，值越小越靠前。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 是否在侧边栏显示。
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// 备注，可为空。
    /// </summary>
    public string? Remark { get; set; }
}
