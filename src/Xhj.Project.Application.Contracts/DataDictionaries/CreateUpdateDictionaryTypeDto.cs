using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典类型新增/编辑入参。
/// </summary>
public class CreateUpdateDictionaryTypeDto
{
    /// <summary>
    /// 字典类型编码，必填且租户内唯一，长度见 <see cref="DataDictionaryConsts.MaxCodeLength"/>。
    /// </summary>
    [Required]
    [MaxLength(DataDictionaryConsts.MaxCodeLength)]
    public string Code { get; set; } = default!;

    /// <summary>
    /// 显示名称，必填，长度见 <see cref="DataDictionaryConsts.MaxNameLength"/>。
    /// </summary>
    [Required]
    [MaxLength(DataDictionaryConsts.MaxNameLength)]
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// 类型说明，可为空。
    /// </summary>
    [MaxLength(DataDictionaryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    /// <summary>
    /// 排序号，值越小越靠前，默认 0。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用，默认 true。
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
