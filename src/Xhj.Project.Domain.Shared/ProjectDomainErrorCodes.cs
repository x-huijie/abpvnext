namespace Xhj.Project;

/// <summary>
/// 项目统一的业务异常错误码。
/// </summary>
/// <remarks>
/// 1. 错误码统一使用 <c>Project:</c> 前缀，才能命中 <c>MapCodeNamespace("Project", typeof(ProjectResource))</c> 的本地化映射；
/// 2. 本地化文件的键必须等于错误码本身（如 "Project:Auth:001"），否则消息会退化成默认的异常文本；
/// 3. 抛出方式统一为 <c>throw new BusinessException(ProjectDomainErrorCodes.Xxx)</c>。
/// </remarks>
public static class ProjectDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    #region 认证/Token

    /// <summary>
    /// 用户名或密码错误：AuthServer 返回 invalid_grant（登录或刷新凭据不正确）时抛出。
    /// </summary>
    public const string InvalidUserNameOrPassword = "Project:Auth:001";

    /// <summary>
    /// 认证服务器配置不完整：缺少 Authority/ClientId/ClientSecret 时抛出。
    /// </summary>
    public const string AuthServerNotConfigured = "Project:Auth:002";

    /// <summary>
    /// 获取访问令牌失败：令牌终结点返回非 invalid_grant 的其他错误（如 invalid_client）时抛出。
    /// </summary>
    public const string TokenRequestFailed = "Project:Auth:003";

    /// <summary>
    /// 刷新访问令牌失败：刷新流程中返回非 invalid_grant 的错误时抛出。
    /// </summary>
    public const string RefreshTokenFailed = "Project:Auth:004";

    /// <summary>
    /// 刷新令牌无效或已过期：用 refresh_token 换取新令牌被拒时抛出。
    /// </summary>
    public const string InvalidRefreshToken = "Project:Auth:005";

    #endregion

    #region 部门（组织机构）

    /// <summary>
    /// 部门编码重复：同一租户内已存在相同 Code 时抛出（Data 中会带 Code）。
    /// </summary>
    public const string DepartmentCodeAlreadyExists = "Project:Department:001";

    /// <summary>
    /// 上级部门不存在：新增/移动时指定的 ParentId 查不到记录时抛出。
    /// </summary>
    public const string ParentDepartmentNotFound = "Project:Department:002";

    /// <summary>
    /// 部门下存在子部门：删除部门时存在下级节点则拒绝删除（Data 中会带 Name）。
    /// </summary>
    public const string DepartmentHasChildren = "Project:Department:003";

    /// <summary>
    /// 非法移动：把部门移动到自身或其子孙节点下时抛出。
    /// </summary>
    public const string CannotMoveDepartmentToChild = "Project:Department:004";

    #endregion

    #region 数据字典

    /// <summary>
    /// 字典类型编码重复：同一租户内已存在相同 Code 时抛出（Data 中会带 Code）。
    /// </summary>
    public const string DictionaryTypeCodeAlreadyExists = "Project:Dictionary:001";

    /// <summary>
    /// 字典类型不存在：按 Id 或 Code 查不到字典类型时抛出。
    /// </summary>
    public const string DictionaryTypeNotFound = "Project:Dictionary:002";

    /// <summary>
    /// 字典项取值重复：同一字典类型下已存在相同 Value 时抛出（Data 中会带 Value）。
    /// </summary>
    public const string DictionaryItemValueAlreadyExists = "Project:Dictionary:003";

    #endregion

    #region 菜单（动态菜单）

    /// <summary>
    /// 菜单标识重复：同一租户内已存在相同 Name 时抛出（Data 中会带 Name）。
    /// </summary>
    public const string MenuNameAlreadyExists = "Project:Menu:001";

    /// <summary>
    /// 上级菜单不存在：新增/移动时指定的 ParentId 查不到记录时抛出。
    /// </summary>
    public const string ParentMenuNotFound = "Project:Menu:002";

    /// <summary>
    /// 菜单下存在子菜单：删除菜单时存在下级节点则拒绝删除（Data 中会带 Name）。
    /// </summary>
    public const string MenuHasChildren = "Project:Menu:003";

    /// <summary>
    /// 非法移动：把菜单移动到自身或其子孙节点下时抛出。
    /// </summary>
    public const string CannotMoveMenuToChild = "Project:Menu:004";

    #endregion

    #region 数据权限

    /// <summary>
    /// 数据权限规则重复：同一角色在同一资源上已存在规则时抛出。
    /// </summary>
    public const string DataPermissionRuleAlreadyExists = "Project:DataPermission:001";

    /// <summary>
    /// 自定义范围未指定部门：Scope 为 Custom 但部门集合为空时抛出。
    /// </summary>
    public const string CustomScopeDepartmentRequired = "Project:DataPermission:002";

    #endregion

    #region 站内消息

    /// <summary>
    /// 消息不存在：按 Id 查不到该用户的站内消息时抛出。
    /// </summary>
    public const string UserNotificationNotFound = "Project:Notification:001";

    #endregion

    #region 文件上传

    /// <summary>
    /// 文件大小超限：超过配置的最大字节数时抛出（Data 中会带 MaxSize）。
    /// </summary>
    public const string FileSizeExceeded = "Project:File:001";

    /// <summary>
    /// 文件类型不允许：扩展名不在白名单内时抛出（Data 中会带 Extension）。
    /// </summary>
    public const string FileExtensionNotAllowed = "Project:File:002";

    /// <summary>
    /// 文件记录不存在：按 Id 查不到文件记录时抛出。
    /// </summary>
    public const string FileNotFound = "Project:File:003";

    #endregion
}
