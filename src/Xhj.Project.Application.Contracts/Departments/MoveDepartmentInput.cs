using System;

namespace Xhj.Project.Departments;

/// <summary>
/// 调整部门层级的入参。
/// </summary>
public class MoveDepartmentInput
{
    /// <summary>
    /// 新的上级部门 Id；为 null 表示提升到根级。
    /// </summary>
    /// <remarks>
    /// 不能指向自身或其子孙节点，否则返回 Project:Department:004。
    /// </remarks>
    public Guid? NewParentId { get; set; }
}
