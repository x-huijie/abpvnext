namespace Xhj.Project.Menus;

/// <summary>
/// 菜单（动态菜单）字段的长度约束，实体、DTO、EF 映射共用一份。
/// </summary>
public static class MenuConsts
{
    /// <summary>
    /// 菜单标识（Name）最大长度，租户内唯一，前端可用作路由 key。
    /// </summary>
    public const int MaxNameLength = 64;

    /// <summary>
    /// 菜单显示名最大长度。
    /// </summary>
    public const int MaxDisplayNameLength = 128;

    /// <summary>
    /// 前端路由地址最大长度。
    /// </summary>
    public const int MaxPathLength = 256;

    /// <summary>
    /// 前端组件路径最大长度。
    /// </summary>
    public const int MaxComponentLength = 256;

    /// <summary>
    /// 菜单图标最大长度。
    /// </summary>
    public const int MaxIconLength = 64;

    /// <summary>
    /// 菜单关联的权限名最大长度，对应 <c>ProjectPermissions</c> 中的常量值。
    /// </summary>
    public const int MaxPermissionLength = 128;

    /// <summary>
    /// 备注最大长度。
    /// </summary>
    public const int MaxRemarkLength = 512;
}
