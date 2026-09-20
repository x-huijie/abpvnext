using System;

namespace Xhj.Project.Menus;

/// <summary>
/// 调整菜单层级的入参。
/// </summary>
public class MoveMenuInput
{
    /// <summary>
    /// 新的上级菜单 Id；为 null 表示提升到顶级。
    /// </summary>
    /// <remarks>
    /// 不能指向自身或其子孙节点，否则返回 Project:Menu:004。
    /// </remarks>
    public Guid? NewParentId { get; set; }
}
