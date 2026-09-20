using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单领域服务：维护菜单树的全部不变式。
/// </summary>
/// <remarks>
/// 跨记录校验（标识唯一、上级存在、子树判断）放在这里，
/// 单实体自身的必填/长度校验放在 <see cref="Menu"/> 内部。
/// </remarks>
public class MenuManager : DomainService
{
    private readonly IRepository<Menu, Guid> _menuRepository;

    /// <summary>
    /// 构造菜单领域服务。
    /// </summary>
    /// <param name="menuRepository">菜单仓储。</param>
    public MenuManager(IRepository<Menu, Guid> menuRepository)
    {
        _menuRepository = menuRepository;
    }

    /// <summary>
    /// 创建菜单：校验标识唯一与上级存在后返回新实体（未持久化）。
    /// </summary>
    /// <param name="name">菜单标识。</param>
    /// <param name="displayName">显示名。</param>
    /// <param name="parentId">上级菜单 Id，可为空。</param>
    /// <param name="path">路由地址。</param>
    /// <param name="component">组件路径。</param>
    /// <param name="icon">图标。</param>
    /// <param name="permission">所需权限名。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="isVisible">是否显示。</param>
    /// <param name="remark">备注。</param>
    /// <returns>新建的菜单聚合根。</returns>
    /// <exception cref="BusinessException">标识重复或上级不存在时抛出。</exception>
    public virtual async Task<Menu> CreateAsync(
        string name,
        string displayName,
        Guid? parentId,
        string? path,
        string? component,
        string? icon,
        string? permission,
        int sort,
        bool isEnabled,
        bool isVisible,
        string? remark)
    {
        await CheckNameAsync(name);
        await EnsureParentExistsAsync(parentId);

        return new Menu(
            GuidGenerator.Create(),
            name,
            displayName,
            parentId,
            path,
            component,
            icon,
            permission,
            sort,
            isEnabled,
            isVisible,
            remark);
    }

    /// <summary>
    /// 变更菜单标识；标识未变化则跳过校验。
    /// </summary>
    /// <param name="menu">目标菜单。</param>
    /// <param name="name">新的菜单标识。</param>
    /// <exception cref="BusinessException">标识已被占用时抛出。</exception>
    public virtual async Task ChangeNameAsync(Menu menu, string name)
    {
        if (string.Equals(menu.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await CheckNameAsync(name, menu.Id);

        menu.SetName(name);
    }

    /// <summary>
    /// 变更上级菜单：不允许移动到自身或其子孙节点下。
    /// </summary>
    /// <param name="menu">待移动的菜单。</param>
    /// <param name="newParentId">新的上级 Id；为 null 表示提升到顶级。</param>
    /// <exception cref="BusinessException">上级不存在，或移动到自身/子孙节点时抛出。</exception>
    public virtual async Task MoveAsync(Menu menu, Guid? newParentId)
    {
        if (!newParentId.HasValue)
        {
            menu.SetParent(null);
            return;
        }

        if (newParentId.Value == menu.Id)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CannotMoveMenuToChild);
        }

        await EnsureParentExistsAsync(newParentId);

        if (await IsDescendantAsync(newParentId.Value, menu.Id))
        {
            throw new BusinessException(ProjectDomainErrorCodes.CannotMoveMenuToChild);
        }

        menu.SetParent(newParentId);
    }

    /// <summary>
    /// 删除前校验：存在子菜单时拒绝删除。
    /// </summary>
    /// <param name="menu">待删除的菜单。</param>
    /// <exception cref="BusinessException">存在子菜单时抛出。</exception>
    public virtual async Task EnsureDeletableAsync(Menu menu)
    {
        var hasChildren = await _menuRepository.AnyAsync(x => x.ParentId == menu.Id);

        if (hasChildren)
        {
            throw new BusinessException(ProjectDomainErrorCodes.MenuHasChildren)
                .WithData("Name", menu.Name);
        }
    }

    /// <summary>
    /// 校验菜单标识在租户内是否可用。
    /// </summary>
    /// <param name="name">待校验的标识。</param>
    /// <param name="exceptId">排除的菜单 Id（更新场景），可为空。</param>
    /// <exception cref="BusinessException">标识已被占用时抛出。</exception>
    private async Task CheckNameAsync(string name, Guid? exceptId = null)
    {
        // 仓储已按当前租户过滤，因此只需比对标识本身
        var exists = exceptId.HasValue
            ? await _menuRepository.AnyAsync(x => x.Name == name && x.Id != exceptId.Value)
            : await _menuRepository.AnyAsync(x => x.Name == name);

        if (exists)
        {
            throw new BusinessException(ProjectDomainErrorCodes.MenuNameAlreadyExists)
                .WithData("Name", name);
        }
    }

    /// <summary>
    /// 校验上级菜单是否存在。
    /// </summary>
    /// <param name="parentId">上级菜单 Id，可为空表示顶级。</param>
    /// <exception cref="BusinessException">上级菜单不存在时抛出。</exception>
    private async Task EnsureParentExistsAsync(Guid? parentId)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        var parent = await _menuRepository.FindAsync(parentId.Value);

        if (parent == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ParentMenuNotFound);
        }
    }

    /// <summary>
    /// 判断 candidateId 是否位于 ancestorId 的子树中。
    /// </summary>
    /// <param name="candidateId">待判断的节点 Id。</param>
    /// <param name="ancestorId">可能的祖先节点 Id。</param>
    /// <returns>candidateId 在 ancestorId 子树中返回 true。</returns>
    /// <remarks>
    /// 菜单数量有限（通常几十到几百），一次性载入后在内存中沿 ParentId 上溯，比递归查库更简单可靠。
    /// </remarks>
    private async Task<bool> IsDescendantAsync(Guid candidateId, Guid ancestorId)
    {
        var menus = await _menuRepository.GetListAsync();
        var current = menus.FirstOrDefault(x => x.Id == candidateId);

        while (current?.ParentId != null)
        {
            if (current.ParentId.Value == ancestorId)
            {
                return true;
            }

            current = menus.FirstOrDefault(x => x.Id == current.ParentId.Value);
        }

        return false;
    }
}
