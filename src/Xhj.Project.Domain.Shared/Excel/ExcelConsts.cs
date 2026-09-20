namespace Xhj.Project.Excel;

/// <summary>
/// Excel 导入导出的通用约束，避免一次性处理过大文件拖垮服务。
/// </summary>
public static class ExcelConsts
{
    /// <summary>
    /// 单次导入允许的最大行数。
    /// </summary>
    public const int MaxImportRows = 5000;

    /// <summary>
    /// 单次导出允许的最大行数。
    /// </summary>
    public const int MaxExportRows = 100000;

    /// <summary>
    /// 默认工作表名称。
    /// </summary>
    public const string DefaultSheetName = "Sheet1";

    /// <summary>
    /// Excel 文件的 MIME 类型（xlsx）。
    /// </summary>
    public const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
