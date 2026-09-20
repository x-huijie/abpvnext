namespace Xhj.Project.Permissions;

public static class ProjectPermissions
{
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

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
