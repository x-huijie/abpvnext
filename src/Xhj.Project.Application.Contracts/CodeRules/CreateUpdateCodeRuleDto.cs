using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则新增/编辑入参。
/// </summary>
public class CreateUpdateCodeRuleDto
{
    /// <summary>
    /// 规则标识，必填且租户内唯一，如 "Order"。
    /// </summary>
    [Required]
    [MaxLength(CodeRuleConsts.MaxCodeLength)]
    public string Code { get; set; } = default!;

    /// <summary>
    /// 规则名称，必填。
    /// </summary>
    [Required]
    [MaxLength(CodeRuleConsts.MaxNameLength)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 单号前缀，可为空。
    /// </summary>
    [MaxLength(CodeRuleConsts.MaxPrefixLength)]
    public string? Prefix { get; set; }

    /// <summary>
    /// 日期格式，如 "yyyyMMdd"；为空表示不带日期段。
    /// </summary>
    [MaxLength(CodeRuleConsts.MaxDateFormatLength)]
    public string? DateFormat { get; set; }

    /// <summary>
    /// 各段之间的分隔符，可为空表示紧凑拼接。
    /// </summary>
    [MaxLength(CodeRuleConsts.MaxSeparatorLength)]
    public string? Separator { get; set; }

    /// <summary>
    /// 流水号位数，默认见 <see cref="CodeRuleConsts.DefaultSerialLength"/>。
    /// </summary>
    public int SerialLength { get; set; } = CodeRuleConsts.DefaultSerialLength;

    /// <summary>
    /// 步长，默认见 <see cref="CodeRuleConsts.DefaultStep"/>。
    /// </summary>
    public int Step { get; set; } = CodeRuleConsts.DefaultStep;

    /// <summary>
    /// 流水号重置周期。
    /// </summary>
    public CodeRuleResetMode ResetMode { get; set; } = CodeRuleResetMode.None;

    /// <summary>
    /// 是否启用，默认 true。
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
