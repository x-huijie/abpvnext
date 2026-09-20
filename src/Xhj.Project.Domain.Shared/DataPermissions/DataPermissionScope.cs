namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限范围，决定用户能看到某个资源的哪些数据。
/// </summary>
public enum DataPermissionScope
{
    /// <summary>
    /// 全部数据，不做任何过滤。
    /// </summary>
    All = 1,

    /// <summary>
    /// 仅本人数据，按创建人 CreatorId 过滤。
    /// </summary>
    Self = 2,

    /// <summary>
    /// 本部门数据，按当前用户所属部门过滤。
    /// </summary>
    Department = 3,

    /// <summary>
    /// 本部门及所有下级部门的数据。
    /// </summary>
    DepartmentAndChildren = 4,

    /// <summary>
    /// 自定义部门集合，按规则中配置的部门 Id 过滤。
    /// </summary>
    Custom = 5
}
