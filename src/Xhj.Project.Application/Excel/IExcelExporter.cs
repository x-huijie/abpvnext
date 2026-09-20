using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xhj.Project.Excel;

/// <summary>
/// Excel 导出器：把任意列表导出为 xlsx 字节流。
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// 导出为 Excel（xlsx）。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    /// <param name="data">待导出的数据。</param>
    /// <param name="sheetName">工作表名，为空时用默认值。</param>
    /// <param name="columnHeaders">列名映射：属性名 → 表头文字，用于输出中文表头。</param>
    /// <returns>xlsx 文件字节。</returns>
    Task<byte[]> ExportAsync<T>(
        IReadOnlyList<T> data,
        string? sheetName = null,
        IDictionary<string, string>? columnHeaders = null)
        where T : class;
}
