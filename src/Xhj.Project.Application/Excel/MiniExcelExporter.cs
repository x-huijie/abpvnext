using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MiniExcelLibs;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Xhj.Project.Excel;

/// <summary>
/// 基于 MiniExcel 的导出实现。
/// </summary>
/// <remarks>
/// 统一先转成 DataTable 再导出：这样可以自由控制列顺序与中文表头，
/// 也避免把导航属性、集合属性误写进表格。
/// </remarks>
public class MiniExcelExporter : IExcelExporter, ITransientDependency
{
    /// <summary>
    /// 导出为 Excel（xlsx）。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    /// <param name="data">待导出的数据。</param>
    /// <param name="sheetName">工作表名，为空时用默认值。</param>
    /// <param name="columnHeaders">列名映射：属性名 → 表头文字。</param>
    /// <returns>xlsx 文件字节。</returns>
    /// <exception cref="BusinessException">数据量超过允许的上限时抛出。</exception>
    public virtual Task<byte[]> ExportAsync<T>(
        IReadOnlyList<T> data,
        string? sheetName = null,
        IDictionary<string, string>? columnHeaders = null)
        where T : class
    {
        if (data == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ExcelContentIsEmpty);
        }

        if (data.Count > ExcelConsts.MaxExportRows)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ExcelRowsExceeded)
                .WithData("MaxRows", ExcelConsts.MaxExportRows);
        }

        var table = BuildDataTable(data, columnHeaders);

        using var stream = new MemoryStream();

        // MiniExcel 的同步写出已足够快，这里不引入异步版本以免阻塞线程池
        MiniExcel.SaveAs(stream, table, sheetName: sheetName ?? ExcelConsts.DefaultSheetName);

        return Task.FromResult(stream.ToArray());
    }

    /// <summary>
    /// 把对象列表转换为 DataTable，列顺序取属性声明顺序。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    /// <param name="data">数据列表。</param>
    /// <param name="columnHeaders">表头映射。</param>
    /// <returns>可用于导出的 DataTable。</returns>
    private static DataTable BuildDataTable<T>(IReadOnlyList<T> data, IDictionary<string, string>? columnHeaders)
    {
        var table = new DataTable();
        var properties = GetExportableProperties<T>();

        foreach (var property in properties)
        {
            var columnName = columnHeaders != null && columnHeaders.TryGetValue(property.Name, out var header)
                ? header
                : property.Name;

            table.Columns.Add(columnName, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        }

        foreach (var item in data)
        {
            var row = table.NewRow();

            foreach (var property in properties)
            {
                row[columnHeaders != null && columnHeaders.TryGetValue(property.Name, out var header)
                    ? header
                    : property.Name] = property.GetValue(item) ?? DBNull.Value;
            }

            table.Rows.Add(row);
        }

        return table;
    }

    /// <summary>
    /// 取可导出的属性：仅公共实例属性中的简单类型与常见值类型。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    /// <returns>属性集合。</returns>
    private static List<PropertyInfo> GetExportableProperties<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
            .Where(x => IsSimpleType(x.PropertyType))
            .ToList();
    }

    /// <summary>
    /// 判断是否可直接写入单元格的类型（基本类型、字符串、DateTime、Guid、decimal 及其可空形式）。
    /// </summary>
    /// <param name="type">属性类型。</param>
    /// <returns>可写入返回 true。</returns>
    private static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive
               || underlyingType.IsEnum
               || underlyingType == typeof(string)
               || underlyingType == typeof(decimal)
               || underlyingType == typeof(DateTime)
               || underlyingType == typeof(Guid);
    }
}
