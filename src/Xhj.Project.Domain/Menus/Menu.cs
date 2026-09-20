using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单（动态菜单）聚合根，通过 <see cref="ParentId"/> 自引用形成多级菜单树。
/// </summary>
/// <remarks>
/// 不变式（由 <see cref="MenuManager"/> 保证）：
/// 1. 菜单标识 Name 在同一租户内唯一；
/// 2. 上级菜单必须存在，且不能是自身或其子孙节点；
/// 3. 存在子菜单时不允许删除。
/// 动态菜单的"可见性"不落库：菜单上配置 Permission 后，由应用服务按当前用户权限实时过滤。
/// </remarks>
public class Menu : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 菜单标识，租户内唯一，长度见 <see cref="MenuConsts.MaxNameLength"/>。
    /// </summary>
    public virtual string Name { get; protected set; } = default!;

    /// <summary>
    /// 菜单显示名，长度见 <see cref="MenuConsts.MaxDisplayNameLength"/>。
    /// </summary>
    public virtual string DisplayName { get; protected set; } = default!;

    /// <summary>
    /// 上级菜单 Id；为 null 表示顶级菜单。
    /// </summary>
    public virtual Guid? ParentId { get; protected set; }

    /// <summary>
    /// 前端路由地址，可为空（纯分组节点没有路由）。
    /// </summary>
    public virtual string? Path { get; protected set; }

    /// <summary>
    /// 前端组件路径，可为空。
    /// </summary>
    public virtual string? Component { get; protected set; }

    /// <summary>
    /// 菜单图标，可为空。
    /// </summary>
    public virtual string? Icon { get; protected set; }

    /// <summary>
    /// 访问该菜单所需的权限名；为空表示登录即可见。
    /// </summary>
    public virtual string? Permission { get; protected set; }

    /// <summary>
    /// 排序号，值越小越靠前。
    /// </summary>
    public virtual int Sort { get; protected set; }

    /// <summary>
    /// 是否启用；停用后不出现在动态菜单中。
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    /// <summary>
    /// 是否在侧边栏显示；false 时可作为隐藏路由（如详情页）。
    /// </summary>
    public virtual bool IsVisible { get; protected set; }

    /// <summary>
    /// 备注说明，可为空。
    /// </summary>
    public virtual string? Remark { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected Menu()
    {
        Name = string.Empty;
        DisplayName = string.Empty;
    }

    /// <summary>
    /// 创建菜单实例。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="name">菜单标识（租户内唯一）。</param>
    /// <param name="displayName">显示名。</param>
    /// <param name="parentId">上级菜单 Id，可为空表示顶级。</param>
    /// <param name="path">前端路由地址，可为空。</param>
    /// <param name="component">前端组件路径，可为空。</param>
    /// <param name="icon">图标，可为空。</param>
    /// <param name="permission">所需权限名，可为空表示登录即可见。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="isVisible">是否显示。</param>
    /// <param name="remark">备注，可为空。</param>
    public Menu(
        Guid id,
        string name,
        string displayName,
        Guid? parentId = null,
        string? path = null,
        string? component = null,
        string? icon = null,
        string? permission = null,
        int sort = 0,
        bool isEnabled = true,
        bool isVisible = true,
        string? remark = null)
        : base(id)
    {
        SetName(name);
        SetDisplayName(displayName);
        SetParent(parentId);
        SetPath(path);
        SetComponent(component);
        SetIcon(icon);
        SetPermission(permission);
        SetRemark(remark);
        Sort = sort;
        IsEnabled = isEnabled;
        IsVisible = isVisible;
    }

    /// <summary>
    /// 修改菜单标识，校验必填与长度（唯一性由 <see cref="MenuManager"/> 校验）。
    /// </summary>
    /// <param name="name">新的菜单标识。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), MenuConsts.MaxNameLength);

        Name = name;
        return this;
    }

    /// <summary>
    /// 修改菜单显示名，校验必填与长度。
    /// </summary>
    /// <param name="displayName">显示名。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetDisplayName(string displayName)
    {
        Check.NotNullOrWhiteSpace(displayName, nameof(displayName), MenuConsts.MaxDisplayNameLength);

        DisplayName = displayName;
        return this;
    }

    /// <summary>
    /// 设置上级菜单；合法性由 <see cref="MenuManager"/> 校验。
    /// </summary>
    /// <param name="parentId">上级菜单 Id，可为空表示顶级。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetParent(Guid? parentId)
    {
        ParentId = parentId;
        return this;
    }

    /// <summary>
    /// 修改前端路由地址。
    /// </summary>
    /// <param name="path">路由地址，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetPath(string? path)
    {
        Path = path;
        return this;
    }

    /// <summary>
    /// 修改前端组件路径。
    /// </summary>
    /// <param name="component">组件路径，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetComponent(string? component)
    {
        Component = component;
        return this;
    }

    /// <summary>
    /// 修改菜单图标。
    /// </summary>
    /// <param name="icon">图标标识，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetIcon(string? icon)
    {
        Icon = icon;
        return this;
    }

    /// <summary>
    /// 修改关联权限名；传入空表示登录即可见。
    /// </summary>
    /// <param name="permission">权限名，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetPermission(string? permission)
    {
        Permission = permission;
        return this;
    }

    /// <summary>
    /// 修改备注。
    /// </summary>
    /// <param name="remark">备注，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetRemark(string? remark)
    {
        Remark = remark;
        return this;
    }

    /// <summary>
    /// 修改排序号。
    /// </summary>
    /// <param name="sort">排序号，值越小越靠前。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetSort(int sort)
    {
        Sort = sort;
        return this;
    }

    /// <summary>
    /// 启用/停用菜单。
    /// </summary>
    /// <param name="isEnabled">true 为启用。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// 设置是否在侧边栏显示。
    /// </summary>
    /// <param name="isVisible">true 为显示。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Menu SetVisible(bool isVisible)
    {
        IsVisible = isVisible;
        return this;
    }
}
