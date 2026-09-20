using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单应用服务：提供菜单 CRUD、菜单树与"当前用户可见菜单"。
/// </summary>
public interface IMenuAppService :
    ICrudAppService<MenuDto, Guid, GetMenuListInput, CreateUpdateMenuDto>
{
    /// <summary>
    /// 菜单树（不分页），供菜单管理页与前端渲染使用。
    /// </summary>
    /// <param name="input">过滤与排序条件，分页参数会被忽略。</param>
    /// <returns>顶级菜单集合，Children 递归嵌套。</returns>
    Task<ListResultDto<MenuTreeDto>> GetTreeAsync(GetMenuListInput input);

    /// <summary>
    /// 获取当前登录用户可见的菜单树（动态菜单核心接口）。
    /// </summary>
    /// <returns>按权限过滤后的菜单树；无可见菜单时返回空集合。</returns>
    /// <remarks>
    /// 过滤规则：仅保留启用且可见的菜单；菜单配置了 Permission 时，当前用户必须拥有该权限。
    /// </remarks>
    Task<ListResultDto<MenuTreeDto>> GetCurrentUserMenusAsync();

    /// <summary>
    /// 调整菜单层级。
    /// </summary>
    /// <param name="id">菜单 Id。</param>
    /// <param name="input">新的上级菜单。</param>
    /// <returns>调整后的菜单。</returns>
    Task<MenuDto> MoveAsync(Guid id, MoveMenuInput input);
}
