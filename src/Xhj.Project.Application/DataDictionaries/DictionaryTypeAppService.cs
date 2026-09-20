using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典类型应用服务：编排用例、维护字典项缓存一致性。
/// </summary>
/// <remarks>
/// 类型编码变更或删除会使对应的字典项缓存失效，因此本服务也持有缓存实例。
/// </remarks>
[Authorize(ProjectPermissions.DataDictionaries.Default)]
public class DictionaryTypeAppService :
    CrudAppService<DictionaryType, DictionaryTypeDto, Guid, GetDictionaryTypeListInput, CreateUpdateDictionaryTypeDto>,
    IDictionaryTypeAppService
{
    private readonly DataDictionaryManager _dataDictionaryManager;
    private readonly IDistributedCache<DictionaryItemCacheItem> _cache;

    /// <summary>
    /// 构造字典类型应用服务。
    /// </summary>
    /// <param name="repository">字典类型仓储。</param>
    /// <param name="dataDictionaryManager">字典领域服务。</param>
    /// <param name="cache">字典项缓存，用于类型变更时失效。</param>
    public DictionaryTypeAppService(
        IRepository<DictionaryType, Guid> repository,
        DataDictionaryManager dataDictionaryManager,
        IDistributedCache<DictionaryItemCacheItem> cache)
        : base(repository)
    {
        _dataDictionaryManager = dataDictionaryManager;
        _cache = cache;
    }

    /// <summary>
    /// 生成当前租户下该类型编码对应的缓存键。
    /// </summary>
    /// <param name="typeCode">字典类型编码。</param>
    /// <returns>缓存键。</returns>
    private string BuildCacheKey(string typeCode)
    {
        return DataDictionaryCacheKey.Build(CurrentTenant.Id, typeCode);
    }

    /// <summary>
    /// 新增字典类型。
    /// </summary>
    /// <param name="input">类型信息。</param>
    /// <returns>新建的字典类型。</returns>
    /// <exception cref="BusinessException">编码重复时抛出。</exception>
    [Authorize(ProjectPermissions.DataDictionaries.Create)]
    public override async Task<DictionaryTypeDto> CreateAsync(CreateUpdateDictionaryTypeDto input)
    {
        var type = await _dataDictionaryManager.CreateTypeAsync(
            input.Code,
            input.DisplayName,
            input.Description,
            input.Sort,
            input.IsEnabled);

        await Repository.InsertAsync(type, autoSave: true);

        return ObjectMapper.Map<DictionaryType, DictionaryTypeDto>(type);
    }

    /// <summary>
    /// 编辑字典类型。
    /// </summary>
    /// <param name="id">字典类型 Id。</param>
    /// <param name="input">新的类型信息。</param>
    /// <returns>更新后的字典类型。</returns>
    [Authorize(ProjectPermissions.DataDictionaries.Update)]
    public override async Task<DictionaryTypeDto> UpdateAsync(Guid id, CreateUpdateDictionaryTypeDto input)
    {
        var type = await Repository.GetAsync(id);

        // 编码是缓存键的一部分，必须在编码变更前用旧值失效缓存
        await _cache.RemoveAsync(BuildCacheKey(type.Code));

        await _dataDictionaryManager.ChangeTypeCodeAsync(type, input.Code);

        type
            .SetDisplayName(input.DisplayName)
            .SetDescription(input.Description)
            .SetSort(input.Sort)
            .SetEnabled(input.IsEnabled);

        await Repository.UpdateAsync(type, autoSave: true);

        return ObjectMapper.Map<DictionaryType, DictionaryTypeDto>(type);
    }

    /// <summary>
    /// 删除字典类型。
    /// </summary>
    /// <param name="id">字典类型 Id。</param>
    /// <exception cref="BusinessException">其下仍存在字典项时抛出。</exception>
    [Authorize(ProjectPermissions.DataDictionaries.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        var type = await Repository.GetAsync(id);

        await _dataDictionaryManager.EnsureTypeDeletableAsync(type);

        await Repository.DeleteAsync(type, autoSave: true);
        await _cache.RemoveAsync(BuildCacheKey(type.Code));
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    protected override async Task<IQueryable<DictionaryType>> CreateFilteredQueryAsync(GetDictionaryTypeListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Code.Contains(input.Filter!) || x.DisplayName.Contains(input.Filter!));
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        return query;
    }
}
