using System;
using Volo.Abp.Application.Services;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典类型应用服务，提供字典类型的增删改查。
/// </summary>
/// <remarks>
/// 删除类型前会校验其下无字典项，并清除该类型对应的缓存。
/// </remarks>
public interface IDictionaryTypeAppService :
    ICrudAppService<DictionaryTypeDto, Guid, GetDictionaryTypeListInput, CreateUpdateDictionaryTypeDto>
{
}
