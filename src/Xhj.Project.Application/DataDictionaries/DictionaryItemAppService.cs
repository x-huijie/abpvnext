using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项应用服务：CRUD + 高频读缓存。
/// </summary>
/// <remarks>
/// 字典属于高频读、低频写数据，因此按类型编码整体缓存；
/// 任何写入操作都必须主动失效对应类型的缓存，避免读到脏数据。
/// </remarks>
[Authorize(ProjectPermissions.DataDictionaries.Default)]
public class DictionaryItemAppService :
    CrudAppService<DictionaryItem, DictionaryItemDto, Guid, GetDictionaryItemListInput, CreateUpdateDictionaryItemDto>,
    IDictionaryItemAppService
{
    private readonly DataDictionaryManager _dataDictionaryManager;
    private readonly IRepository<DictionaryType, Guid> _dictionaryTypeRepository;
    private readonly IDistributedCache<DictionaryItemCacheItem> _cache;

    /// <summary>
    /// 构造字典项应用服务。
    /// </summary>
    /// <param name="repository">字典项仓储。</param>
    /// <param name="dataDictionaryManager">字典领域服务。</param>
    /// <param name="dictionaryTypeRepository">字典类型仓储，用于失效缓存时查编码。</param>
    /// <param name="cache">字典项分布式缓存。</param>
    public DictionaryItemAppService(
        IRepository<DictionaryItem, Guid> repository,
        DataDictionaryManager dataDictionaryManager,
        IRepository<DictionaryType, Guid> dictionaryTypeRepository,
        IDistributedCache<DictionaryItemCacheItem> cache)
        : base(repository)
    {
        _dataDictionaryManager = dataDictionaryManager;
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _cache = cache;
    }

    /// <summary>
    /// 新增字典项。
    /// </summary>
    /// <param name="input">字典项信息。</param>
    /// <returns>新建的字典项。</returns>
    /// <exception cref="BusinessException">类型不存在或取值重复时抛出。</exception>
    [Authorize(ProjectPermissions.DataDictionaries.Create)]
    public override async Task<DictionaryItemDto> CreateAsync(CreateUpdateDictionaryItemDto input)
    {
        var item = await _dataDictionaryManager.CreateItemAsync(
            input.DictionaryTypeId,
            input.Label,
            input.Value,
            input.Sort,
            input.IsEnabled,
            input.Description);

        await Repository.InsertAsync(item, autoSave: true);
        await InvalidateCacheAsync(input.DictionaryTypeId);

        return ObjectMapper.Map<DictionaryItem, DictionaryItemDto>(item);
    }

    /// <summary>
    /// 编辑字典项。
    /// </summary>
    /// <param name="id">字典项 Id。</param>
    /// <param name="input">新的字典项信息。</param>
    /// <returns>更新后的字典项。</returns>
    /// <exception cref="BusinessException">取值在同类型内重复时抛出。</exception>
    [Authorize(ProjectPermissions.DataDictionaries.Update)]
    public override async Task<DictionaryItemDto> UpdateAsync(Guid id, CreateUpdateDictionaryItemDto input)
    {
        var item = await Repository.GetAsync(id);

        // 若类型发生变更，旧类型缓存也要失效，因此先记录原类型
        var oldTypeId = item.DictionaryTypeId;

        await _dataDictionaryManager.ChangeItemValueAsync(item, input.Label, input.Value);

        item
            .SetType(input.DictionaryTypeId)
            .SetSort(input.Sort)
            .SetEnabled(input.IsEnabled)
            .SetDescription(input.Description);

        await Repository.UpdateAsync(item, autoSave: true);

        await InvalidateCacheAsync(oldTypeId);

        if (oldTypeId != item.DictionaryTypeId)
        {
            await InvalidateCacheAsync(item.DictionaryTypeId);
        }

        return ObjectMapper.Map<DictionaryItem, DictionaryItemDto>(item);
    }

    /// <summary>
    /// 删除字典项。
    /// </summary>
    /// <param name="id">字典项 Id。</param>
    [Authorize(ProjectPermissions.DataDictionaries.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        var item = await Repository.GetAsync(id);

        await Repository.DeleteAsync(item, autoSave: true);
        await InvalidateCacheAsync(item.DictionaryTypeId);
    }

    /// <summary>
    /// 高频读接口：按字典类型编码取启用项，走分布式缓存。
    /// </summary>
    /// <param name="typeCode">字典类型编码。</param>
    /// <returns>该类型下已启用的字典项。</returns>
    /// <exception cref="BusinessException">字典类型不存在时抛出。</exception>
    public virtual async Task<ListResultDto<DictionaryItemDto>> GetByTypeCodeAsync(string typeCode)
    {
        Check.NotNullOrWhiteSpace(typeCode, nameof(typeCode));

        // 缓存不可用时（如 Redis 故障）直接回源，保证功能可用
        var cacheItem = await _cache.GetOrAddAsync(
            BuildCacheKey(typeCode),
            () => LoadItemsAsync(typeCode),
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(DataDictionaryConsts.CacheDurationMinutes)
            }) ?? await LoadItemsAsync(typeCode);

        return new ListResultDto<DictionaryItemDto>(cacheItem.Items);
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    protected override async Task<IQueryable<DictionaryItem>> CreateFilteredQueryAsync(GetDictionaryItemListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (input.DictionaryTypeId.HasValue)
        {
            query = query.Where(x => x.DictionaryTypeId == input.DictionaryTypeId.Value);
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Label.Contains(input.Filter!) || x.Value.Contains(input.Filter!));
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        return query;
    }

    /// <summary>
    /// 缓存回源：按类型编码查出该类型的全部启用项并排序。
    /// </summary>
    /// <param name="typeCode">字典类型编码。</param>
    /// <returns>缓存内容。</returns>
    private async Task<DictionaryItemCacheItem> LoadItemsAsync(string typeCode)
    {
        var type = await _dataDictionaryManager.GetTypeByCodeAsync(typeCode);

        var query = await Repository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query
                .Where(x => x.DictionaryTypeId == type.Id && x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Label));

        return new DictionaryItemCacheItem
        {
            Items = items.Select(x => ObjectMapper.Map<DictionaryItem, DictionaryItemDto>(x)).ToList()
        };
    }

    /// <summary>
    /// 失效指定字典类型对应的缓存。
    /// </summary>
    /// <param name="dictionaryTypeId">字典类型 Id。</param>
    private async Task InvalidateCacheAsync(Guid dictionaryTypeId)
    {
        var type = await _dictionaryTypeRepository.FindAsync(dictionaryTypeId);

        if (type == null)
        {
            return;
        }

        await _cache.RemoveAsync(BuildCacheKey(type.Code));
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
}
