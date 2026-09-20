using System;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项缓存键生成器，供字典类型与字典项两个应用服务共用。
/// </summary>
public static class DataDictionaryCacheKey
{
    /// <summary>
    /// 生成按租户隔离的缓存键。
    /// </summary>
    /// <param name="tenantId">当前租户 Id；host 为 null。</param>
    /// <param name="typeCode">字典类型编码。</param>
    /// <returns>形如 <c>Project:DictionaryItems:{tenant}:{typeCode}</c> 的缓存键。</returns>
    /// <remarks>
    /// 缓存键必须包含租户标识，否则不同租户的同名字典会互相覆盖。
    /// </remarks>
    public static string Build(Guid? tenantId, string typeCode)
    {
        var tenantKey = tenantId?.ToString("N") ?? "host";

        return $"Project:DictionaryItems:{tenantKey}:{typeCode}";
    }
}
