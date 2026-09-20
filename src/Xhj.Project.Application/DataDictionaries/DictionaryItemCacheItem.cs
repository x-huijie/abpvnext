using System.Collections.Generic;

namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 字典项缓存内容，按类型编码整体缓存一个类型的全部启用项。
/// </summary>
/// <remarks>
/// 放的是 DTO 而不是实体，避免把领域对象序列化进 Redis 造成耦合与膨胀。
/// </remarks>
public class DictionaryItemCacheItem
{
    /// <summary>
    /// 该字典类型下的启用项，已按 Sort、Label 排序。
    /// </summary>
    public List<DictionaryItemDto> Items { get; set; } = new();
}
