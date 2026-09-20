using System;
using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项新增/编辑入参。
/// </summary>
public class CreateUpdateDictionaryItemDto
{
    /// <summary>
    /// 所属字典类型 Id，必填，必须指向已存在的类型。
    /// </summary>
    [Required]
    public Guid DictionaryTypeId { get; set; }

    /// <summary>
    /// 展示文本，必填，长度见 <see cref="DataDictionaryConsts.MaxLabelLength"/>。
    /// </summary>
    [Required]
    [MaxLength(DataDictionaryConsts.MaxLabelLength)]
    public string Label { get; set; } = default!;

    /// <summary>
    /// 实际取值，必填且在同类型内唯一，长度见 <see cref="DataDictionaryConsts.MaxValueLength"/>。
    /// </summary>
    [Required]
    [MaxLength(DataDictionaryConsts.MaxValueLength)]
    public string Value { get; set; } = default!;

    /// <summary>
    /// 排序号，值越小越靠前，默认 0。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用，默认 true；停用后不出现在高频查询接口中。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 字典项说明，可为空。
    /// </summary>
    [MaxLength(DataDictionaryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
