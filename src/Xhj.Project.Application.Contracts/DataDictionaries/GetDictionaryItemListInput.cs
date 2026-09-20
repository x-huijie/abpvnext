using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项列表查询入参。
/// </summary>
public class GetDictionaryItemListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 按字典类型过滤；为 null 表示跨类型查询。
    /// </summary>
    public Guid? DictionaryTypeId { get; set; }

    /// <summary>
    /// 显示名或取值关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 按启用状态过滤；为 null 表示不过滤。
    /// </summary>
    public bool? IsEnabled { get; set; }
}
