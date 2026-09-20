using Xhj.Project.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Xhj.Project.Permissions;

/// <summary>
/// 权限定义提供者：把 <see cref="ProjectPermissions"/> 中的常量注册到 ABP 权限系统。
/// </summary>
public class ProjectPermissionDefinitionProvider : PermissionDefinitionProvider
{
    /// <summary>
    /// 定义本项目的全部权限。
    /// </summary>
    /// <param name="context">权限定义上下文。</param>
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ProjectPermissions.GroupName);

        var departmentPermission = myGroup.AddPermission(ProjectPermissions.Departments.Default,
            L("Permission:Departments"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Create, L("Permission:Departments.Create"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Update, L("Permission:Departments.Update"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Delete, L("Permission:Departments.Delete"));
        departmentPermission.AddChild(ProjectPermissions.Departments.Move, L("Permission:Departments.Move"));
        departmentPermission.AddChild(ProjectPermissions.Departments.ManageMembers,
            L("Permission:Departments.ManageMembers"));

        var dictionaryPermission = myGroup.AddPermission(ProjectPermissions.DataDictionaries.Default,
            L("Permission:DataDictionaries"));
        dictionaryPermission.AddChild(ProjectPermissions.DataDictionaries.Create,
            L("Permission:DataDictionaries.Create"));
        dictionaryPermission.AddChild(ProjectPermissions.DataDictionaries.Update,
            L("Permission:DataDictionaries.Update"));
        dictionaryPermission.AddChild(ProjectPermissions.DataDictionaries.Delete,
            L("Permission:DataDictionaries.Delete"));

        var menuPermission = myGroup.AddPermission(ProjectPermissions.Menus.Default, L("Permission:Menus"));
        menuPermission.AddChild(ProjectPermissions.Menus.Create, L("Permission:Menus.Create"));
        menuPermission.AddChild(ProjectPermissions.Menus.Update, L("Permission:Menus.Update"));
        menuPermission.AddChild(ProjectPermissions.Menus.Delete, L("Permission:Menus.Delete"));
        menuPermission.AddChild(ProjectPermissions.Menus.Move, L("Permission:Menus.Move"));

        var dataPermission = myGroup.AddPermission(ProjectPermissions.DataPermissions.Default,
            L("Permission:DataPermissions"));
        dataPermission.AddChild(ProjectPermissions.DataPermissions.Create, L("Permission:DataPermissions.Create"));
        dataPermission.AddChild(ProjectPermissions.DataPermissions.Update, L("Permission:DataPermissions.Update"));
        dataPermission.AddChild(ProjectPermissions.DataPermissions.Delete, L("Permission:DataPermissions.Delete"));

        var filePermission = myGroup.AddPermission(ProjectPermissions.Files.Default, L("Permission:Files"));
        filePermission.AddChild(ProjectPermissions.Files.Upload, L("Permission:Files.Upload"));
        filePermission.AddChild(ProjectPermissions.Files.Delete, L("Permission:Files.Delete"));

        myGroup.AddPermission(ProjectPermissions.OperationLogs.Default, L("Permission:OperationLogs"));

        var codeRulePermission = myGroup.AddPermission(ProjectPermissions.CodeRules.Default, L("Permission:CodeRules"));
        codeRulePermission.AddChild(ProjectPermissions.CodeRules.Create, L("Permission:CodeRules.Create"));
        codeRulePermission.AddChild(ProjectPermissions.CodeRules.Update, L("Permission:CodeRules.Update"));
        codeRulePermission.AddChild(ProjectPermissions.CodeRules.Delete, L("Permission:CodeRules.Delete"));

        var notificationPermission = myGroup.AddPermission(ProjectPermissions.Notifications.Default,
            L("Permission:Notifications"));
        notificationPermission.AddChild(ProjectPermissions.Notifications.Send, L("Permission:Notifications.Send"));
    }

    /// <summary>
    /// 取权限显示名的可本地化字符串。
    /// </summary>
    /// <param name="name">本地化键。</param>
    /// <returns>可本地化字符串。</returns>
    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProjectResource>(name);
    }
}
