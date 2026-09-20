namespace Xhj.Project.DataDictionaries;

/// <summary>
/// 数据字典字段的长度约束与缓存策略，实体、DTO、EF 映射共用一份。
/// </summary>
public static class DataDictionaryConsts
{
    /// <summary>
    /// 字典类型编码最大长度。
    /// </summary>
    public const int MaxCodeLength = 64;

    /// <summary>
    /// 字典类型显示名最大长度。
    /// </summary>
    public const int MaxNameLength = 128;

    /// <summary>
    /// 字典项显示文本最大长度。
    /// </summary>
    public const int MaxLabelLength = 128;

    /// <summary>
    /// 字典项取值最大长度。
    /// </summary>
    public const int MaxValueLength = 512;

    /// <summary>
    /// 描述信息最大长度。
    /// </summary>
    public const int MaxDescriptionLength = 512;

    /// <summary>
    /// 字典项缓存时长（分钟）。
    /// </summary>
    /// <remarks>
    /// 字典属于高频读、低频写的数据，按类型编码整体缓存；写入时主动失效，避免长时间脏读。
    /// </remarks>
    public const int CacheDurationMinutes = 30;
}
