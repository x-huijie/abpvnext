using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项聚合根，归属于某个 <see cref="DictionaryType"/>（以 Id 引用，不做导航属性）。
/// </summary>
/// <remarks>
/// 不变式（由 <see cref="DataDictionaryManager"/> 保证）：
/// 1. Label 与 Value 必填且不超长；
/// 2. Value 在同一字典类型内不允许重复；
/// 3. 字典类型必须存在。
/// 高频读场景按类型编码整体缓存，写入后由应用服务主动失效缓存。
/// </remarks>
public class DictionaryItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 所属字典类型 Id。
    /// </summary>
    public virtual Guid DictionaryTypeId { get; protected set; }

    /// <summary>
    /// 展示文本（如"男"），长度见 <see cref="DataDictionaryConsts.MaxLabelLength"/>。
    /// </summary>
    public virtual string Label { get; protected set; } = default!;

    /// <summary>
    /// 实际取值（如"1"），类型内唯一，长度见 <see cref="DataDictionaryConsts.MaxValueLength"/>。
    /// </summary>
    public virtual string Value { get; protected set; } = default!;

    /// <summary>
    /// 排序号，值越小越靠前。
    /// </summary>
    public virtual int Sort { get; protected set; }

    /// <summary>
    /// 是否启用；仅启用项会出现在高频查询接口中。
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    /// <summary>
    /// 字典项说明，可为空。
    /// </summary>
    public virtual string? Description { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected DictionaryItem()
    {
    }

    /// <summary>
    /// 创建字典项实例。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="dictionaryTypeId">所属字典类型 Id。</param>
    /// <param name="label">展示文本。</param>
    /// <param name="value">实际取值。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="isEnabled">是否启用，默认启用。</param>
    /// <param name="description">说明，可为空。</param>
    public DictionaryItem(
        Guid id,
        Guid dictionaryTypeId,
        string label,
        string value,
        int sort = 0,
        bool isEnabled = true,
        string? description = null)
        : base(id)
    {
        SetType(dictionaryTypeId);
        SetLabel(label);
        SetValue(value);
        Sort = sort;
        IsEnabled = isEnabled;
        SetDescription(description);
    }

    /// <summary>
    /// 设置所属字典类型。
    /// </summary>
    /// <param name="dictionaryTypeId">字典类型 Id。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryItem SetType(Guid dictionaryTypeId)
    {
        Check.NotNull(dictionaryTypeId, nameof(dictionaryTypeId));

        DictionaryTypeId = dictionaryTypeId;
        return this;
    }

    /// <summary>
    /// 修改展示文本，校验必填与长度。
    /// </summary>
    /// <param name="label">展示文本。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryItem SetLabel(string label)
    {
        Check.NotNullOrWhiteSpace(label, nameof(label), DataDictionaryConsts.MaxLabelLength);

        Label = label;
        return this;
    }

    /// <summary>
    /// 修改取值，校验必填与长度（类型内唯一性由领域服务校验）。
    /// </summary>
    /// <param name="value">实际取值。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryItem SetValue(string value)
    {
        Check.NotNullOrWhiteSpace(value, nameof(value), DataDictionaryConsts.MaxValueLength);

        Value = value;
        return this;
    }

    /// <summary>
    /// 修改排序号。
    /// </summary>
    /// <param name="sort">排序号，值越小越靠前。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryItem SetSort(int sort)
    {
        Sort = sort;
        return this;
    }

    /// <summary>
    /// 启用/停用该字典项。
    /// </summary>
    /// <param name="isEnabled">true 为启用。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryItem SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// 修改字典项说明。
    /// </summary>
    /// <param name="description">说明，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryItem SetDescription(string? description)
    {
        Description = description;
        return this;
    }
}
