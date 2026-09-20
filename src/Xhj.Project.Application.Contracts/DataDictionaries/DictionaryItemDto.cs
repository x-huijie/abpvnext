using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项输出 DTO。
/// </summary>
public class DictionaryItemDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 所属字典类型 Id。
    /// </summary>
    public Guid DictionaryTypeId { get; set; }

    /// <summary>
    /// 展示文本，如"男"。
    /// </summary>
    public string Label { get; set; } = default!;

    /// <summary>
    /// 实际取值，如"1"，类型内唯一。
    /// </summary>
    public string Value { get; set; } = default!;

    /// <summary>
    /// 排序号，值越小越靠前。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用；高频接口只返回启用项。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 字典项说明，可为空。
    /// </summary>
    public string? Description { get; set; }
}
