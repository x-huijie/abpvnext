using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单应用服务：菜单 CRUD、菜单树，以及按权限过滤的动态菜单。
/// </summary>
/// <remarks>
/// 菜单的业务规则下沉到 <see cref="Menu"/> 与 <see cref="MenuManager"/>；
/// 动态菜单的可见性在这里根据当前用户的权限实时计算，不落库。
/// </remarks>
[Authorize(ProjectPermissions.Menus.Default)]
public class MenuAppService :
    CrudAppService<Menu, MenuDto, Guid, GetMenuListInput, CreateUpdateMenuDto>,
    IMenuAppService
{
    private readonly MenuManager _menuManager;
    private readonly IPermissionChecker _permissionChecker;

    /// <summary>
    /// 构造菜单应用服务。
    /// </summary>
    /// <param name="repository">菜单仓储。</param>
    /// <param name="menuManager">菜单领域服务。</param>
    /// <param name="permissionChecker">权限检查器，用于动态菜单过滤。</param>
    public MenuAppService(
        IRepository<Menu, Guid> repository,
        MenuManager menuManager,
        IPermissionChecker permissionChecker)
        : base(repository)
    {
        _menuManager = menuManager;
        _permissionChecker = permissionChecker;
    }

    /// <summary>
    /// 新增菜单。
    /// </summary>
    /// <param name="input">菜单信息。</param>
    /// <returns>新建的菜单。</returns>
    /// <exception cref="BusinessException">标识重复或上级不存在时抛出。</exception>
    [Authorize(ProjectPermissions.Menus.Create)]
    public override async Task<MenuDto> CreateAsync(CreateUpdateMenuDto input)
    {
        var menu = await _menuManager.CreateAsync(
            input.Name,
            input.DisplayName,
            input.ParentId,
            input.Path,
            input.Component,
            input.Icon,
            input.Permission,
            input.Sort,
            input.IsEnabled,
            input.IsVisible,
            input.Remark);

        await Repository.InsertAsync(menu, autoSave: true);

        return ObjectMapper.Map<Menu, MenuDto>(menu);
    }

    /// <summary>
    /// 编辑菜单基本信息。
    /// </summary>
    /// <param name="id">菜单 Id。</param>
    /// <param name="input">新的菜单信息。</param>
    /// <returns>更新后的菜单。</returns>
    [Authorize(ProjectPermissions.Menus.Update)]
    public override async Task<MenuDto> UpdateAsync(Guid id, CreateUpdateMenuDto input)
    {
        var menu = await Repository.GetAsync(id);

        await _menuManager.ChangeNameAsync(menu, input.Name);

        menu
            .SetDisplayName(input.DisplayName)
            .SetPath(input.Path)
            .SetComponent(input.Component)
            .SetIcon(input.Icon)
            .SetPermission(input.Permission)
            .SetRemark(input.Remark)
            .SetSort(input.Sort)
            .SetEnabled(input.IsEnabled)
            .SetVisible(input.IsVisible);

        // 上级菜单变更走独立的 Move 接口，避免编辑时被误改层级
        await Repository.UpdateAsync(menu, autoSave: true);

        return ObjectMapper.Map<Menu, MenuDto>(menu);
    }

    /// <summary>
    /// 删除菜单。
    /// </summary>
    /// <param name="id">菜单 Id。</param>
    /// <exception cref="BusinessException">存在子菜单时抛出。</exception>
    [Authorize(ProjectPermissions.Menus.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        var menu = await Repository.GetAsync(id);

        await _menuManager.EnsureDeletableAsync(menu);

        await Repository.DeleteAsync(menu, autoSave: true);
    }

    /// <summary>
    /// 查询菜单树，一次性返回全部层级。
    /// </summary>
    /// <param name="input">过滤条件；分页参数会被忽略。</param>
    /// <returns>顶级菜单及其递归子节点。</returns>
    public virtual async Task<ListResultDto<MenuTreeDto>> GetTreeAsync(GetMenuListInput input)
    {
        // 树结构一次性加载，不分页
        input.MaxResultCount = int.MaxValue;

        var query = await CreateFilteredQueryAsync(input);
        query = ApplySorting(query, input);

        var menus = await AsyncExecuter.ToListAsync(query);
        var nodes = menus
            .Select(x => ObjectMapper.Map<Menu, MenuTreeDto>(x))
            .ToList();

        return new ListResultDto<MenuTreeDto>(BuildTree(nodes));
    }

    /// <summary>
    /// 获取当前登录用户可见的菜单树（动态菜单）。
    /// </summary>
    /// <returns>按权限过滤后的菜单树。</returns>
    /// <remarks>
    /// 过滤分两步：先去掉停用/隐藏的菜单，再对配置了 Permission 的菜单做权限校验；
    /// 父菜单被过滤后，其子菜单会被提升为根节点，避免出现空壳分组。
    /// </remarks>
    [AllowAnonymous]
    public virtual async Task<ListResultDto<MenuTreeDto>> GetCurrentUserMenusAsync()
    {
        var query = await Repository.GetQueryableAsync();

        var menus = await AsyncExecuter.ToListAsync(
            query
                .Where(x => x.IsEnabled && x.IsVisible)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name));

        var visibleMenus = new List<Menu>();

        foreach (var menu in menus)
        {
            // 未配置权限的菜单视为登录即可见
            if (menu.Permission.IsNullOrWhiteSpace())
            {
                visibleMenus.Add(menu);
                continue;
            }

            if (await _permissionChecker.IsGrantedAsync(menu.Permission!))
            {
                visibleMenus.Add(menu);
            }
        }

        var nodes = visibleMenus
            .Select(x => ObjectMapper.Map<Menu, MenuTreeDto>(x))
            .ToList();

        return new ListResultDto<MenuTreeDto>(BuildTree(nodes));
    }

    /// <summary>
    /// 调整菜单层级。
    /// </summary>
    /// <param name="id">菜单 Id。</param>
    /// <param name="input">新的上级菜单。</param>
    /// <returns>调整后的菜单。</returns>
    /// <exception cref="BusinessException">上级不存在或移动到自身/子孙时抛出。</exception>
    public virtual async Task<MenuDto> MoveAsync(Guid id, MoveMenuInput input)
    {
        var menu = await Repository.GetAsync(id);

        await _menuManager.MoveAsync(menu, input.NewParentId);

        await Repository.UpdateAsync(menu, autoSave: true);

        return ObjectMapper.Map<Menu, MenuDto>(menu);
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    protected override async Task<IQueryable<Menu>> CreateFilteredQueryAsync(GetMenuListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.Contains(input.Filter!) || x.DisplayName.Contains(input.Filter!));
        }

        if (input.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == input.ParentId.Value);
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        return query;
    }

    /// <summary>
    /// 由扁平列表组装树：ParentId 不在集合内的节点视为根节点。
    /// </summary>
    /// <param name="nodes">菜单扁平列表。</param>
    /// <returns>已排序的根节点集合。</returns>
    private static List<MenuTreeDto> BuildTree(List<MenuTreeDto> nodes)
    {
        var childrenLookup = nodes.ToLookup(x => x.ParentId);

        foreach (var node in nodes)
        {
            node.Children = childrenLookup[node.Id]
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name)
                .ToList();
        }

        return nodes
            .Where(x => !x.ParentId.HasValue || !nodes.Any(p => p.Id == x.ParentId))
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .ToList();
    }
}
