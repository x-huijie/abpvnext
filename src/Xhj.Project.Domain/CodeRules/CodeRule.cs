using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则聚合根：描述业务单号（订单号、申请单号等）的生成方式。
/// </summary>
/// <remarks>
/// 单号结构：<c>前缀 + 分隔符 + 日期部分 + 分隔符 + 流水号</c>，空段自动跳过。
/// 流水号按 <see cref="ResetMode"/> 周期性重置，且由 <see cref="CodeGenerator"/> 在分布式锁保护下递增。
/// </remarks>
public class CodeRule : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 规则标识，业务侧唯一，如 "Order"。
    /// </summary>
    public virtual string Code { get; protected set; } = default!;

    /// <summary>
    /// 规则名称，如 "销售订单号"。
    /// </summary>
    public virtual string Name { get; protected set; } = default!;

    /// <summary>
    /// 单号前缀，可为空，如 "SO"。
    /// </summary>
    public virtual string? Prefix { get; protected set; }

    /// <summary>
    /// 日期部分格式，可为空表示不带日期，如 "yyyyMMdd"。
    /// </summary>
    public virtual string? DateFormat { get; protected set; }

    /// <summary>
    /// 各段之间的分隔符，可为空，默认为空字符串（紧凑单号）。
    /// </summary>
    public virtual string? Separator { get; protected set; }

    /// <summary>
    /// 流水号位数，不足左侧补 0。
    /// </summary>
    public virtual int SerialLength { get; protected set; }

    /// <summary>
    /// 每次生成时流水号的步长。
    /// </summary>
    public virtual int Step { get; protected set; }

    /// <summary>
    /// 当前流水号，下一次生成时在此基础上加 <see cref="Step"/>。
    /// </summary>
    public virtual long CurrentSerial { get; protected set; }

    /// <summary>
    /// 流水号重置周期。
    /// </summary>
    public virtual CodeRuleResetMode ResetMode { get; protected set; }

    /// <summary>
    /// 上一次生成时的周期键（如 "20260920"），用于判断是否需要重置流水号。
    /// </summary>
    public virtual string? LastResetKey { get; protected set; }

    /// <summary>
    /// 是否启用；停用后不允许再生成单号。
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected CodeRule()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// 创建编码规则。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="code">规则标识。</param>
    /// <param name="name">规则名称。</param>
    /// <param name="prefix">单号前缀，可为空。</param>
    /// <param name="dateFormat">日期格式，可为空。</param>
    /// <param name="separator">分隔符，可为空。</param>
    /// <param name="serialLength">流水号位数。</param>
    /// <param name="step">步长。</param>
    /// <param name="resetMode">重置周期。</param>
    /// <param name="isEnabled">是否启用。</param>
    public CodeRule(
        Guid id,
        string code,
        string name,
        string? prefix = null,
        string? dateFormat = "yyyyMMdd",
        string? separator = null,
        int serialLength = CodeRuleConsts.DefaultSerialLength,
        int step = CodeRuleConsts.DefaultStep,
        CodeRuleResetMode resetMode = CodeRuleResetMode.None,
        bool isEnabled = true)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetPrefix(prefix);
        SetDateFormat(dateFormat);
        SetSeparator(separator);
        SetSerialLength(serialLength);
        SetStep(step);
        ResetMode = resetMode;
        IsEnabled = isEnabled;
        CurrentSerial = 0;
    }

    /// <summary>
    /// 修改规则标识。
    /// </summary>
    /// <param name="code">规则标识。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), CodeRuleConsts.MaxCodeLength);

        Code = code;
        return this;
    }

    /// <summary>
    /// 修改规则名称。
    /// </summary>
    /// <param name="name">规则名称。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), CodeRuleConsts.MaxNameLength);

        Name = name;
        return this;
    }

    /// <summary>
    /// 修改单号前缀。
    /// </summary>
    /// <param name="prefix">前缀，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetPrefix(string? prefix)
    {
        Prefix = prefix;
        return this;
    }

    /// <summary>
    /// 修改日期格式。
    /// </summary>
    /// <param name="dateFormat">日期格式，可为空表示不带日期段。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetDateFormat(string? dateFormat)
    {
        DateFormat = dateFormat;
        return this;
    }

    /// <summary>
    /// 修改分隔符。
    /// </summary>
    /// <param name="separator">分隔符，可为空表示紧凑拼接。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetSeparator(string? separator)
    {
        Separator = separator;
        return this;
    }

    /// <summary>
    /// 修改流水号位数，取值 1~<see cref="CodeRuleConsts.MaxSerialLength"/>。
    /// </summary>
    /// <param name="serialLength">位数。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetSerialLength(int serialLength)
    {
        if (serialLength < 1 || serialLength > CodeRuleConsts.MaxSerialLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(serialLength),
                $"流水号位数必须在 1 到 {CodeRuleConsts.MaxSerialLength} 之间。");
        }

        SerialLength = serialLength;
        return this;
    }

    /// <summary>
    /// 修改步长，必须大于 0。
    /// </summary>
    /// <param name="step">步长。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetStep(int step)
    {
        if (step <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(step), "步长必须大于 0。");
        }

        Step = step;
        return this;
    }

    /// <summary>
    /// 修改重置周期。
    /// </summary>
    /// <param name="resetMode">重置周期。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetResetMode(CodeRuleResetMode resetMode)
    {
        ResetMode = resetMode;
        return this;
    }

    /// <summary>
    /// 启用/停用规则。
    /// </summary>
    /// <param name="isEnabled">true 为启用。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual CodeRule SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// 计算指定时间对应的周期键；<see cref="CodeRuleResetMode.None"/> 恒返回空字符串。
    /// </summary>
    /// <param name="time">当前时间。</param>
    /// <returns>周期键字符串。</returns>
    public virtual string ComputeResetKey(DateTime time)
    {
        return ResetMode switch
        {
            CodeRuleResetMode.Daily => time.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            CodeRuleResetMode.Monthly => time.ToString("yyyyMM", CultureInfo.InvariantCulture),
            CodeRuleResetMode.Yearly => time.ToString("yyyy", CultureInfo.InvariantCulture),
            _ => string.Empty
        };
    }

    /// <summary>
    /// 取下一个单号：必要时先按周期重置流水号，再按步长递增并拼接各段。
    /// </summary>
    /// <param name="time">当前时间，用于日期段与周期判断。</param>
    /// <returns>生成的单号。</returns>
    public virtual string GenerateNext(DateTime time)
    {
        var resetKey = ComputeResetKey(time);

        // 跨周期则流水号归零，保证"每天/每月/每年从 1 开始"
        if (!string.Equals(LastResetKey, resetKey, StringComparison.Ordinal))
        {
            CurrentSerial = 0;
            LastResetKey = resetKey;
        }

        CurrentSerial += Step;

        return BuildCode(time);
    }

    /// <summary>
    /// 按当前配置拼接单号，空段自动跳过。
    /// </summary>
    /// <param name="time">当前时间，用于日期段。</param>
    /// <returns>拼接后的单号。</returns>
    private string BuildCode(DateTime time)
    {
        var segments = new List<string>();

        if (!Prefix.IsNullOrWhiteSpace())
        {
            segments.Add(Prefix!);
        }

        if (!DateFormat.IsNullOrWhiteSpace())
        {
            segments.Add(time.ToString(DateFormat, CultureInfo.InvariantCulture));
        }

        segments.Add(CurrentSerial.ToString($"D{SerialLength}", CultureInfo.InvariantCulture));

        return string.Join(Separator ?? string.Empty, segments.Where(x => !x.IsNullOrWhiteSpace()));
    }
}
