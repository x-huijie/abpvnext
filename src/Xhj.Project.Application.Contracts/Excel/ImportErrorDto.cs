namespace Xhj.Project.Excel;

/// <summary>
/// 导入过程中的单行错误明细。
/// </summary>
public class ImportErrorDto
{
    /// <summary>
    /// 出错行号（数据行序号，从 1 开始）。
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// 该行的关键内容，便于用户在表格中定位。
    /// </summary>
    public string? RowContent { get; set; }

    /// <summary>
    /// 错误原因。
    /// </summary>
    public string Message { get; set; } = default!;
}
