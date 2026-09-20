using Volo.Abp.Application.Dtos;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典类型列表查询入参。
/// </summary>
public class GetDictionaryTypeListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 编码或显示名关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 按启用状态过滤；为 null 表示不过滤。
    /// </summary>
    public bool? IsEnabled { get; set; }
}
