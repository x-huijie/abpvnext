using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Menus;

/// <summary>
/// 菜单树节点，供前端渲染动态菜单。
/// </summary>
/// <remarks>
/// <see cref="Children"/> 由应用服务组装，不参与对象映射。
/// </remarks>
public class MenuTreeDto : MenuDto
{
    /// <summary>
    /// 直接子菜单集合，已排序；叶子节点为空集合。
    /// </summary>
    public List<MenuTreeDto> Children { get; set; } = new();
}
