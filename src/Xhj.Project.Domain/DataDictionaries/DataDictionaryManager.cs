using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 数据字典领域服务：保证类型编码唯一、字典项在同类型内取值唯一、类型必须存在。
/// </summary>
/// <remarks>
/// 与部门一样，跨记录校验放在领域服务；单条记录的必填/长度校验放在实体内部。
/// </remarks>
public class DataDictionaryManager : DomainService
{
    private readonly IRepository<DictionaryType, Guid> _dictionaryTypeRepository;
    private readonly IRepository<DictionaryItem, Guid> _dictionaryItemRepository;

    /// <summary>
    /// 构造字典领域服务。
    /// </summary>
    /// <param name="dictionaryTypeRepository">字典类型仓储。</param>
    /// <param name="dictionaryItemRepository">字典项仓储。</param>
    public DataDictionaryManager(
        IRepository<DictionaryType, Guid> dictionaryTypeRepository,
        IRepository<DictionaryItem, Guid> dictionaryItemRepository)
    {
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _dictionaryItemRepository = dictionaryItemRepository;
    }

    /// <summary>
    /// 创建字典类型（未持久化）。
    /// </summary>
    /// <param name="code">类型编码。</param>
    /// <param name="displayName">显示名。</param>
    /// <param name="description">说明，可为空。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <returns>新建的字典类型。</returns>
    /// <exception cref="BusinessException">编码重复时抛出。</exception>
    public virtual async Task<DictionaryType> CreateTypeAsync(
        string code,
        string displayName,
        string? description,
        int sort,
        bool isEnabled)
    {
        await CheckTypeCodeAsync(code);

        return new DictionaryType(GuidGenerator.Create(), code, displayName, description, sort, isEnabled);
    }

    /// <summary>
    /// 变更字典类型编码；编码未变化则跳过校验。
    /// </summary>
    /// <param name="type">目标字典类型。</param>
    /// <param name="code">新编码。</param>
    /// <exception cref="BusinessException">编码已被占用时抛出。</exception>
    public virtual async Task ChangeTypeCodeAsync(DictionaryType type, string code)
    {
        if (string.Equals(type.Code, code, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await CheckTypeCodeAsync(code, type.Id);

        type.SetCode(code);
    }

    /// <summary>
    /// 按 Id 获取字典类型，不存在时抛业务异常。
    /// </summary>
    /// <param name="id">字典类型 Id。</param>
    /// <returns>字典类型。</returns>
    /// <exception cref="BusinessException">不存在时抛出。</exception>
    public virtual async Task<DictionaryType> GetTypeAsync(Guid id)
    {
        var type = await _dictionaryTypeRepository.FindAsync(id);

        return type ?? throw new BusinessException(ProjectDomainErrorCodes.DictionaryTypeNotFound)
            .WithData("Id", id);
    }

    /// <summary>
    /// 按编码获取字典类型，不存在时抛业务异常。
    /// </summary>
    /// <param name="code">字典类型编码。</param>
    /// <returns>字典类型。</returns>
    /// <exception cref="BusinessException">不存在时抛出。</exception>
    public virtual async Task<DictionaryType> GetTypeByCodeAsync(string code)
    {
        var type = await _dictionaryTypeRepository.FindAsync(x => x.Code == code);

        return type ?? throw new BusinessException(ProjectDomainErrorCodes.DictionaryTypeNotFound)
            .WithData("Code", code);
    }

    /// <summary>
    /// 创建字典项（未持久化）。
    /// </summary>
    /// <param name="dictionaryTypeId">所属字典类型 Id。</param>
    /// <param name="label">展示文本。</param>
    /// <param name="value">实际取值。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="description">说明，可为空。</param>
    /// <returns>新建的字典项。</returns>
    /// <exception cref="BusinessException">类型不存在或取值重复时抛出。</exception>
    public virtual async Task<DictionaryItem> CreateItemAsync(
        Guid dictionaryTypeId,
        string label,
        string value,
        int sort,
        bool isEnabled,
        string? description)
    {
        await GetTypeAsync(dictionaryTypeId);
        await CheckItemValueAsync(dictionaryTypeId, value);

        return new DictionaryItem(GuidGenerator.Create(), dictionaryTypeId, label, value, sort, isEnabled, description);
    }

    /// <summary>
    /// 变更字典项的展示文本与取值。
    /// </summary>
    /// <param name="item">目标字典项。</param>
    /// <param name="label">新的展示文本。</param>
    /// <param name="value">新的取值。</param>
    /// <exception cref="BusinessException">取值在同类型内重复时抛出。</exception>
    public virtual async Task ChangeItemValueAsync(DictionaryItem item, string label, string value)
    {
        if (!string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase))
        {
            await CheckItemValueAsync(item.DictionaryTypeId, value, item.Id);
        }

        item.SetLabel(label);
        item.SetValue(value);
    }

    /// <summary>
    /// 删除字典类型前校验其下是否仍有字典项。
    /// </summary>
    /// <param name="type">待删除的字典类型。</param>
    /// <exception cref="BusinessException">仍存在字典项时抛出。</exception>
    public virtual async Task EnsureTypeDeletableAsync(DictionaryType type)
    {
        var hasItems = await _dictionaryItemRepository.AnyAsync(x => x.DictionaryTypeId == type.Id);

        if (hasItems)
        {
            throw new BusinessException(ProjectDomainErrorCodes.DictionaryTypeNotFound)
                .WithData("Message", $"字典类型 {type.Code} 下仍存在字典项，不能删除。");
        }
    }

    /// <summary>
    /// 校验字典类型编码在租户内是否可用。
    /// </summary>
    /// <param name="code">待校验编码。</param>
    /// <param name="exceptId">排除的类型 Id（更新场景），可为空。</param>
    /// <exception cref="BusinessException">编码已被占用时抛出。</exception>
    private async Task CheckTypeCodeAsync(string code, Guid? exceptId = null)
    {
        var exists = exceptId.HasValue
            ? await _dictionaryTypeRepository.AnyAsync(x => x.Code == code && x.Id != exceptId.Value)
            : await _dictionaryTypeRepository.AnyAsync(x => x.Code == code);

        if (exists)
        {
            throw new BusinessException(ProjectDomainErrorCodes.DictionaryTypeCodeAlreadyExists)
                .WithData("Code", code);
        }
    }

    /// <summary>
    /// 校验字典项取值在同一类型内是否可用。
    /// </summary>
    /// <param name="dictionaryTypeId">所属字典类型 Id。</param>
    /// <param name="value">待校验取值。</param>
    /// <param name="exceptId">排除的字典项 Id（更新场景），可为空。</param>
    /// <exception cref="BusinessException">取值重复时抛出。</exception>
    private async Task CheckItemValueAsync(Guid dictionaryTypeId, string value, Guid? exceptId = null)
    {
        var exists = exceptId.HasValue
            ? await _dictionaryItemRepository.AnyAsync(x =>
                x.DictionaryTypeId == dictionaryTypeId && x.Value == value && x.Id != exceptId.Value)
            : await _dictionaryItemRepository.AnyAsync(x =>
                x.DictionaryTypeId == dictionaryTypeId && x.Value == value);

        if (exists)
        {
            throw new BusinessException(ProjectDomainErrorCodes.DictionaryItemValueAlreadyExists)
                .WithData("Value", value);
        }
    }
}
