using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则输出 DTO。
/// </summary>
public class CodeRuleDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 规则标识，业务侧唯一。
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// 规则名称。
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 单号前缀，可为空。
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// 日期格式，可为空表示不带日期段。
    /// </summary>
    public string? DateFormat { get; set; }

    /// <summary>
    /// 分隔符，可为空表示紧凑拼接。
    /// </summary>
    public string? Separator { get; set; }

    /// <summary>
    /// 流水号位数。
    /// </summary>
    public int SerialLength { get; set; }

    /// <summary>
    /// 步长。
    /// </summary>
    public int Step { get; set; }

    /// <summary>
    /// 当前流水号。
    /// </summary>
    public long CurrentSerial { get; set; }

    /// <summary>
    /// 流水号重置周期。
    /// </summary>
    public CodeRuleResetMode ResetMode { get; set; }

    /// <summary>
    /// 上一次生成时的周期键。
    /// </summary>
    public string? LastResetKey { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; }
}
