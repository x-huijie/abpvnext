namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限相关的字段约束与约定，实体、DTO、EF 映射共用一份。
/// </summary>
public static class DataPermissionConsts
{
    /// <summary>
    /// 资源标识最大长度，建议使用业务模块名，如 "Products"。
    /// </summary>
    public const int MaxResourceKeyLength = 128;

    /// <summary>
    /// 角色名最大长度（冗余存储，便于列表展示）。
    /// </summary>
    public const int MaxRoleNameLength = 256;

    /// <summary>
    /// 自定义部门 Id 集合序列化后的最大长度。
    /// </summary>
    /// <remarks>
    /// 以逗号分隔的 Guid 存储，单个 Guid 36 字符，4096 约可容纳 100 个部门。
    /// </remarks>
    public const int MaxDepartmentIdsLength = 4096;

    /// <summary>
    /// 部门 Id 之间使用的分隔符。
    /// </summary>
    public const string DepartmentIdSeparator = ",";
}
