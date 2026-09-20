namespace Xhj.Project.Permissions;

/// <summary>
/// 项目权限常量，与 <see cref="ProjectPermissionDefinitionProvider"/> 中的定义一一对应。
/// </summary>
public static class ProjectPermissions
{
    /// <summary>
    /// 权限组名，所有权限均挂在该组下。
    /// </summary>
    public const string GroupName = "Project";

    /// <summary>
    /// 组织机构（部门）管理。
    /// </summary>
    public static class Departments
    {
        public const string Default = GroupName + ".Departments";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Move = Default + ".Move";
        public const string ManageMembers = Default + ".ManageMembers";
    }

    /// <summary>
    /// 数据字典管理。
    /// </summary>
    public static class DataDictionaries
    {
        public const string Default = GroupName + ".DataDictionaries";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 菜单（动态菜单）管理。
    /// </summary>
    public static class Menus
    {
        public const string Default = GroupName + ".Menus";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Move = Default + ".Move";
    }

    /// <summary>
    /// 数据权限规则管理。
    /// </summary>
    public static class DataPermissions
    {
        public const string Default = GroupName + ".DataPermissions";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 文件上传管理。
    /// </summary>
    public static class Files
    {
        public const string Default = GroupName + ".Files";
        public const string Upload = Default + ".Upload";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 操作日志查询（手动埋点的重要业务操作）。
    /// </summary>
    public static class OperationLogs
    {
        public const string Default = GroupName + ".OperationLogs";
    }

    /// <summary>
    /// 站内消息。
    /// </summary>
    public static class Notifications
    {
        public const string Default = GroupName + ".Notifications";
        public const string Send = Default + ".Send";
    }

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
