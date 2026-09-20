using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项应用服务，提供字典项的增删改查与高频读取。
/// </summary>
public interface IDictionaryItemAppService :
    ICrudAppService<DictionaryItemDto, Guid, GetDictionaryItemListInput, CreateUpdateDictionaryItemDto>
{
    /// <summary>
    /// 高频读：按字典类型编码取启用项，走分布式缓存。
    /// </summary>
    /// <param name="typeCode">字典类型编码，如 "Gender"。</param>
    /// <returns>该类型下已启用的字典项，按 Sort、Label 排序。</returns>
    Task<ListResultDto<DictionaryItemDto>> GetByTypeCodeAsync(string typeCode);
}
