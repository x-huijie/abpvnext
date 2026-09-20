using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Xhj.Project.DataDictionaries;
using Xhj.Project.DataPermissions;
using Xhj.Project.Departments;
using Xhj.Project.Files;
using Xhj.Project.Menus;
using Xhj.Project.OperationLogs;

namespace Xhj.Project;

/// <summary>
/// 应用层的对象映射定义（Mapperly 在编译期据此生成映射代码）。
/// </summary>
/// <remarks>
/// 由 <c>ProjectApplicationModule</c> 中的 AddMapperlyObjectMapper 注册；
/// 新增映射时在此追加 partial 方法即可，无需手写赋值代码。
/// </remarks>
[Mapper]
public partial class ProjectApplicationMappers
{
    /* You can configure your Mapperly mapping configuration here.
     * Alternatively, you can split your mapping configurations
     * into multiple mapper classes for a better organization. */

    /// <summary>
    /// 部门实体转部门 DTO。
    /// </summary>
    /// <param name="department">部门实体。</param>
    /// <returns>部门 DTO。</returns>
    // ExtraProperties / ConcurrencyStamp 属于 ABP 基础设施属性，不进入 DTO
    [MapperIgnoreSource(nameof(Department.ExtraProperties))]
    [MapperIgnoreSource(nameof(Department.ConcurrencyStamp))]
    public partial DepartmentDto MapDepartmentToDto(Department department);

    /// <summary>
    /// 部门实体转部门树节点。
    /// </summary>
    /// <param name="department">部门实体。</param>
    /// <returns>树节点，Children 由应用服务组装。</returns>
    [MapperIgnoreSource(nameof(Department.ExtraProperties))]
    [MapperIgnoreSource(nameof(Department.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(DepartmentTreeDto.Children))]
    public partial DepartmentTreeDto MapDepartmentToTreeDto(Department department);

    /// <summary>
    /// 部门成员实体转 DTO。
    /// </summary>
    /// <param name="member">部门成员实体。</param>
    /// <returns>部门成员 DTO。</returns>
    [MapperIgnoreSource(nameof(DepartmentMember.ExtraProperties))]
    [MapperIgnoreSource(nameof(DepartmentMember.ConcurrencyStamp))]
    public partial DepartmentMemberDto MapDepartmentMemberToDto(DepartmentMember member);

    /// <summary>
    /// 字典类型实体转 DTO。
    /// </summary>
    /// <param name="dictionaryType">字典类型实体。</param>
    /// <returns>字典类型 DTO。</returns>
    [MapperIgnoreSource(nameof(DictionaryType.ExtraProperties))]
    [MapperIgnoreSource(nameof(DictionaryType.ConcurrencyStamp))]
    public partial DictionaryTypeDto MapDictionaryTypeToDto(DictionaryType dictionaryType);

    /// <summary>
    /// 字典项实体转 DTO。
    /// </summary>
    /// <param name="dictionaryItem">字典项实体。</param>
    /// <returns>字典项 DTO。</returns>
    [MapperIgnoreSource(nameof(DictionaryItem.ExtraProperties))]
    [MapperIgnoreSource(nameof(DictionaryItem.ConcurrencyStamp))]
    public partial DictionaryItemDto MapDictionaryItemToDto(DictionaryItem dictionaryItem);

    /// <summary>
    /// 菜单实体转菜单 DTO。
    /// </summary>
    /// <param name="menu">菜单实体。</param>
    /// <returns>菜单 DTO。</returns>
    [MapperIgnoreSource(nameof(Menu.ExtraProperties))]
    [MapperIgnoreSource(nameof(Menu.ConcurrencyStamp))]
    public partial MenuDto MapMenuToDto(Menu menu);

    /// <summary>
    /// 菜单实体转菜单树节点。
    /// </summary>
    /// <param name="menu">菜单实体。</param>
    /// <returns>树节点，Children 由应用服务组装。</returns>
    [MapperIgnoreSource(nameof(Menu.ExtraProperties))]
    [MapperIgnoreSource(nameof(Menu.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(MenuTreeDto.Children))]
    public partial MenuTreeDto MapMenuToTreeDto(Menu menu);

    /// <summary>
    /// 数据权限规则实体转 DTO。
    /// </summary>
    /// <param name="rule">数据权限规则实体。</param>
    /// <returns>规则 DTO，DepartmentIds 由字符串解析而来。</returns>
    /// <remarks>
    /// 源属性 DepartmentIds 是逗号分隔字符串，DTO 中是集合，
    /// 二者类型不同无法自动映射，因此忽略后由映射后处理补齐。
    /// </remarks>
    [MapperIgnoreSource(nameof(DataPermissionRule.ExtraProperties))]
    [MapperIgnoreSource(nameof(DataPermissionRule.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(DataPermissionRule.DepartmentIds))]
    [MapperIgnoreTarget(nameof(DataPermissionRuleDto.DepartmentIds))]
    public partial DataPermissionRuleDto MapDataPermissionRuleToDto(DataPermissionRule rule);

    /// <summary>
    /// 操作日志实体转 DTO。
    /// </summary>
    /// <param name="operationLog">操作日志实体。</param>
    /// <returns>操作日志 DTO（不含 UserAgent，避免列表响应过大）。</returns>
    [MapperIgnoreSource(nameof(OperationLog.ExtraProperties))]
    [MapperIgnoreSource(nameof(OperationLog.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(OperationLog.UserAgent))]
    public partial OperationLogDto MapOperationLogToDto(OperationLog operationLog);

    /// <summary>
    /// 文件记录实体转 DTO。
    /// </summary>
    /// <param name="fileItem">文件记录实体。</param>
    /// <returns>文件 DTO。</returns>
    [MapperIgnoreSource(nameof(FileItem.ExtraProperties))]
    [MapperIgnoreSource(nameof(FileItem.ConcurrencyStamp))]
    public partial FileItemDto MapFileItemToDto(FileItem fileItem);
}
