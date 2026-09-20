using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典类型输出 DTO。
/// </summary>
public class DictionaryTypeDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 字典类型编码，业务侧唯一。
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// 显示名称，如"性别"。
    /// </summary>
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// 类型说明，可为空。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序号，值越小越靠前。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; }
}
