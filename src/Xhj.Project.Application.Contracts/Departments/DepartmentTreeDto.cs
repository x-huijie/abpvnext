using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门树节点，供前端树形控件/级联下拉使用。
/// </summary>
/// <remarks>
/// <see cref="Children"/> 由应用服务在内存中组装，不参与对象映射（Mapperly 已忽略该目标成员）。
/// </remarks>
public class DepartmentTreeDto : DepartmentDto
{
    /// <summary>
    /// 直接下级部门集合，已按 Sort、Name 排序；叶子节点为空集合。
    /// </summary>
    public List<DepartmentTreeDto> Children { get; set; } = new();
}
