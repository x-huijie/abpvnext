namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则中流水号的重置周期。
/// </summary>
public enum CodeRuleResetMode
{
    /// <summary>
    /// 永不重置，流水号一直累加。
    /// </summary>
    None = 1,

    /// <summary>
    /// 每天从 1 重新开始。
    /// </summary>
    Daily = 2,

    /// <summary>
    /// 每月从 1 重新开始。
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// 每年从 1 重新开始。
    /// </summary>
    Yearly = 4
}
