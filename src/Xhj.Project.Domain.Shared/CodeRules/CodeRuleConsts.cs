namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则字段的长度约束与默认值。
/// </summary>
public static class CodeRuleConsts
{
    /// <summary>
    /// 规则标识最大长度，业务侧唯一，如 "Order"。
    /// </summary>
    public const int MaxCodeLength = 64;

    /// <summary>
    /// 规则名称最大长度。
    /// </summary>
    public const int MaxNameLength = 128;

    /// <summary>
    /// 单号前缀最大长度，如 "SO"。
    /// </summary>
    public const int MaxPrefixLength = 32;

    /// <summary>
    /// 日期格式最大长度，如 "yyyyMMdd"。
    /// </summary>
    public const int MaxDateFormatLength = 32;

    /// <summary>
    /// 各段之间的分隔符最大长度。
    /// </summary>
    public const int MaxSeparatorLength = 8;

    /// <summary>
    /// 流水号默认位数。
    /// </summary>
    public const int DefaultSerialLength = 6;

    /// <summary>
    /// 流水号最大位数（受 long 取值范围约束）。
    /// </summary>
    public const int MaxSerialLength = 18;

    /// <summary>
    /// 默认步长。
    /// </summary>
    public const int DefaultStep = 1;

    /// <summary>
    /// 单次批量生成的默认上限，防止一次占用过多号段。
    /// </summary>
    public const int MaxBatchGenerateCount = 1000;
}
