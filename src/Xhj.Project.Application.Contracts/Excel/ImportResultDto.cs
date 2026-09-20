using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Excel;

/// <summary>
/// 批量导入的结果：总行数、成功数、失败数以及逐行错误明细。
/// </summary>
public class ImportResultDto : EntityDto
{
    /// <summary>
    /// 解析出的总行数。
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功导入的行数。
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败的行数。
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 失败明细，行号从表头之后的第 1 行开始计数。
    /// </summary>
    public List<ImportErrorDto> Errors { get; set; } = new();
}
