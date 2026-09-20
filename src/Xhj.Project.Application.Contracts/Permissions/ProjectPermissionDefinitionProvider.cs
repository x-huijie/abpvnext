using Xhj.Project.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Xhj.Project.Permissions;

public class ProjectPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ProjectPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ProjectPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProjectResource>(name);
    }
}
