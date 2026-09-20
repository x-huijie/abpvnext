using Volo.Abp.Application.Dtos;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则列表查询入参。
/// </summary>
public class GetCodeRuleListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 规则标识或名称关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 按启用状态过滤；为 null 表示不过滤。
    /// </summary>
    public bool? IsEnabled { get; set; }
}
