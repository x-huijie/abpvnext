using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Files;

/// <summary>
/// 文件列表查询入参。
/// </summary>
public class GetFileListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 文件名关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 按虚拟目录过滤（精确匹配）；为空表示不过滤。
    /// </summary>
    public string? Directory { get; set; }
}
