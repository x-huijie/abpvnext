using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典类型聚合根，例如"性别""单据状态"。
/// </summary>
/// <remarks>
/// 不变式（由 <see cref="DataDictionaryManager"/> 保证）：
/// 1. Code 是业务侧唯一标识，同一租户内不允许重复；
/// 2. 显示名必填且不超过约定长度；
/// 3. 类型下仍存在字典项时不允许删除。
/// </remarks>
public class DictionaryType : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 字典类型编码，业务侧唯一，长度见 <see cref="DataDictionaryConsts.MaxCodeLength"/>。
    /// </summary>
    /// <remarks>高频查询以该编码作为缓存键的一部分，因此不建议频繁变更。</remarks>
    public virtual string Code { get; protected set; } = default!;

    /// <summary>
    /// 字典类型显示名（如"性别"），长度见 <see cref="DataDictionaryConsts.MaxNameLength"/>。
    /// </summary>
    public virtual string DisplayName { get; protected set; } = default!;

    /// <summary>
    /// 类型说明，可为空。
    /// </summary>
    public virtual string? Description { get; protected set; }

    /// <summary>
    /// 排序号，值越小越靠前。
    /// </summary>
    public virtual int Sort { get; protected set; }

    /// <summary>
    /// 是否启用；停用后对应的字典项不再对外提供。
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected DictionaryType()
    {
    }

    /// <summary>
    /// 创建字典类型实例。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="code">字典类型编码。</param>
    /// <param name="displayName">显示名。</param>
    /// <param name="description">说明，可为空。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="isEnabled">是否启用，默认启用。</param>
    public DictionaryType(
        Guid id,
        string code,
        string displayName,
        string? description = null,
        int sort = 0,
        bool isEnabled = true)
        : base(id)
    {
        SetCode(code);
        SetDisplayName(displayName);
        SetDescription(description);
        Sort = sort;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// 修改字典类型编码，校验必填与长度（唯一性由领域服务校验）。
    /// </summary>
    /// <param name="code">新的编码。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryType SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), DataDictionaryConsts.MaxCodeLength);

        Code = code;
        return this;
    }

    /// <summary>
    /// 修改显示名，校验必填与长度。
    /// </summary>
    /// <param name="displayName">新的显示名。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryType SetDisplayName(string displayName)
    {
        Check.NotNullOrWhiteSpace(displayName, nameof(displayName), DataDictionaryConsts.MaxNameLength);

        DisplayName = displayName;
        return this;
    }

    /// <summary>
    /// 修改类型说明。
    /// </summary>
    /// <param name="description">说明，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryType SetDescription(string? description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    /// 修改排序号。
    /// </summary>
    /// <param name="sort">排序号，值越小越靠前。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryType SetSort(int sort)
    {
        Sort = sort;
        return this;
    }

    /// <summary>
    /// 启用/停用该字典类型。
    /// </summary>
    /// <param name="isEnabled">true 为启用。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DictionaryType SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }
}
