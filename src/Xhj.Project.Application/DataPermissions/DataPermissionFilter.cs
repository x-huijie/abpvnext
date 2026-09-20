using System;
using System.Collections.Generic;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限解析结果：描述当前用户对某个资源能看到的数据范围。
/// </summary>
/// <remarks>
/// 业务查询用 <see cref="DataPermissionQueryExtensions"/> 把该结果应用为 EF 过滤条件。
/// </remarks>
public class DataPermissionFilter
{
    /// <summary>
    /// 最终生效的数据范围；多规则合并后可能为 <see cref="DataPermissionScope.Custom"/>（按部门集合过滤）。
    /// </summary>
    public DataPermissionScope Scope { get; set; } = DataPermissionScope.All;

    /// <summary>
    /// 允许访问的部门 Id 集合；非部门维度时为空集合。
    /// </summary>
    public List<Guid> DepartmentIds { get; set; } = new();

    /// <summary>
    /// 当前用户 Id；范围为 <see cref="DataPermissionScope.Self"/> 时用于过滤创建人。
    /// </summary>
    public Guid? CurrentUserId { get; set; }

    /// <summary>
    /// 是否未命中任何规则；未命中时默认按"全部"放行，业务可据此收紧为"仅本人"。
    /// </summary>
    public bool HasNoRule { get; set; }

    /// <summary>
    /// 是否不做任何过滤（<see cref="DataPermissionScope.All"/>）。
    /// </summary>
    public bool IsAll => Scope == DataPermissionScope.All;

    /// <summary>
    /// 构造一个"不做过滤"的结果。
    /// </summary>
    /// <param name="hasNoRule">是否因未命中规则而不做过滤。</param>
    /// <returns>放行全部数据的过滤器。</returns>
    public static DataPermissionFilter All(bool hasNoRule = true)
    {
        return new DataPermissionFilter
        {
            Scope = DataPermissionScope.All,
            HasNoRule = hasNoRule
        };
    }
}
