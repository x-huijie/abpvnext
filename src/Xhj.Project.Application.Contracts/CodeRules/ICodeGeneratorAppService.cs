using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码生成器应用服务：对外统一封装"生成业务单号"的入口。
/// </summary>
/// <remarks>
/// 业务侧只需按规则标识取号（如订单号 "Order"、申请单号 "Apply"），
/// 不必关心流水号存储、周期重置与并发控制。
/// </remarks>
public interface ICodeGeneratorAppService
{
    /// <summary>
    /// 生成一个业务单号。
    /// </summary>
    /// <param name="ruleCode">规则标识，如 "Order"。</param>
    /// <returns>单号。</returns>
    Task<string> GenerateAsync(string ruleCode);

    /// <summary>
    /// 批量生成连续的业务单号。
    /// </summary>
    /// <param name="ruleCode">规则标识。</param>
    /// <param name="count">生成数量，上限见 <see cref="CodeRuleConsts.MaxBatchGenerateCount"/>。</param>
    /// <returns>单号列表。</returns>
    Task<List<string>> GenerateBatchAsync(string ruleCode, int count);
}
