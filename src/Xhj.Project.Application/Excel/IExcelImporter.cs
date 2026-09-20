using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xhj.Project.Excel;

/// <summary>
/// Excel 导入器：把 xlsx 首个工作表解析为强类型列表。
/// </summary>
public interface IExcelImporter
{
    /// <summary>
    /// 读取 Excel 并映射为指定类型。
    /// </summary>
    /// <typeparam name="T">目标类型，属性名需与表头一致（或使用 MiniExcel 的列特性）。</typeparam>
    /// <param name="stream">文件流。</param>
    /// <param name="sheetName">工作表名，为空时取第一个工作表。</param>
    /// <returns>解析出的数据行。</returns>
    Task<List<T>> ImportAsync<T>(Stream stream, string? sheetName = null)
        where T : class, new();
}
