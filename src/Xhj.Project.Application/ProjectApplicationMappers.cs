using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Xhj.Project.DataDictionaries;
using Xhj.Project.Departments;

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
}
