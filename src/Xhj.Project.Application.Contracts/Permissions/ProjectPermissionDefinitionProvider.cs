using Xhj.Project.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Xhj.Project.Permissions;

public class ProjectPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ProjectPermissions.GroupName);

        var departmentPermission = myGroup.AddPermission(ProjectPermissions.Departments.Default,
            L("Permission:Departments"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Create, L("Permission:Departments.Create"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Update, L("Permission:Departments.Update"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Delete, L("Permission:Departments.Delete"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Move, L("Permission:Departments.Move"));

        var dictionaryPermission = myGroup.AddPermission(ProjectPermissions.DataDictionaries.Default,
            L("Permission:DataDictionaries"));
        dictionaryPermission.AddChild(ProjectPermissions.DataDictionaries.Create,
            L("Permission:DataDictionaries.Create"));
        dictionaryPermission.AddChild(ProjectPermissions.DataDictionaries.Update,
            L("Permission:DataDictionaries.Update"));
        dictionaryPermission.AddChild(ProjectPermissions.DataDictionaries.Delete,
            L("Permission:DataDictionaries.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProjectResource>(name);
    }
}
