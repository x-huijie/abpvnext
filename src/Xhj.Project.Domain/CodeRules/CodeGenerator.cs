using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Medallion.Threading;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码生成器：按规则生成业务单号（订单号、申请单号等）。
/// </summary>
/// <remarks>
/// 并发安全：单号必须全局唯一，因此生成过程用分布式锁按规则标识串行化。
/// 锁由 Host 侧注册（Redis 实现），单实例部署时也不会出现重复号。
/// </remarks>
public class CodeGenerator : DomainService
{
    /// <summary>
    /// 获取编码锁的超时时间，超时直接失败而不无限等待。
    /// </summary>
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    private readonly IRepository<CodeRule, Guid> _codeRuleRepository;
    private readonly IDistributedLockProvider _distributedLockProvider;

    /// <summary>
    /// 构造编码生成器。
    /// </summary>
    /// <param name="codeRuleRepository">编码规则仓储。</param>
    /// <param name="distributedLockProvider">分布式锁提供者。</param>
    public CodeGenerator(
        IRepository<CodeRule, Guid> codeRuleRepository,
        IDistributedLockProvider distributedLockProvider)
    {
        _codeRuleRepository = codeRuleRepository;
        _distributedLockProvider = distributedLockProvider;
    }

    /// <summary>
    /// 生成单个业务单号。
    /// </summary>
    /// <param name="ruleCode">规则标识，如 "Order"。</param>
    /// <returns>生成的单号。</returns>
    /// <exception cref="BusinessException">规则不存在或已停用时抛出。</exception>
    public virtual async Task<string> GenerateAsync(string ruleCode)
    {
        var codes = await GenerateAsync(ruleCode, 1);

        return codes[0];
    }

    /// <summary>
    /// 批量生成业务单号，号段连续且不重复。
    /// </summary>
    /// <param name="ruleCode">规则标识。</param>
    /// <param name="count">生成数量，上限见 <see cref="CodeRuleConsts.MaxBatchGenerateCount"/>。</param>
    /// <returns>单号列表，按生成顺序排列。</returns>
    /// <exception cref="BusinessException">规则不存在、已停用或数量超限时抛出。</exception>
    public virtual async Task<List<string>> GenerateAsync(string ruleCode, int count)
    {
        if (count <= 0 || count > CodeRuleConsts.MaxBatchGenerateCount)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CodeRuleNotFound)
                .WithData("Message", $"单次生成数量必须在 1 到 {CodeRuleConsts.MaxBatchGenerateCount} 之间。");
        }

        // 同一规则的生成过程必须串行，否则并发下会拿到重复流水号
        var distributedLock = _distributedLockProvider.CreateLock($"Project:CodeRule:{ruleCode}");

        await using var lockHandle = await distributedLock.TryAcquireAsync(LockTimeout);

        if (lockHandle == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CodeRuleNotFound)
                .WithData("Message", $"获取编码规则 {ruleCode} 的锁失败，请稍后重试。");
        }

        var rule = await _codeRuleRepository.FindAsync(x => x.Code == ruleCode)
            ?? throw new BusinessException(ProjectDomainErrorCodes.CodeRuleNotFound).WithData("Code", ruleCode);

        if (!rule.IsEnabled)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CodeRuleDisabled).WithData("Code", ruleCode);
        }

        var codes = new List<string>(count);
        var now = DateTime.Now;

        for (var i = 0; i < count; i++)
        {
            codes.Add(rule.GenerateNext(now));
        }

        await _codeRuleRepository.UpdateAsync(rule, autoSave: true);

        return codes;
    }
}
