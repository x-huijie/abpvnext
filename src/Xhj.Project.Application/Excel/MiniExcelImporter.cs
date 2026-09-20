using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MiniExcelLibs;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Xhj.Project.Excel;

/// <summary>
/// 基于 MiniExcel 的导入实现。
/// </summary>
/// <remarks>
/// 只做"文件 → 强类型列表"的数据转换，业务校验与落库交给调用方，
/// 这样同一份导入能力可以被任意业务复用。
/// </remarks>
public class MiniExcelImporter : IExcelImporter, ITransientDependency
{
    /// <summary>
    /// 读取 Excel 并映射为指定类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="stream">文件流。</param>
    /// <param name="sheetName">工作表名，为空时取第一个工作表。</param>
    /// <returns>解析出的数据行。</returns>
    /// <exception cref="BusinessException">内容为空或行数超限时抛出。</exception>
    public virtual Task<List<T>> ImportAsync<T>(Stream stream, string? sheetName = null)
        where T : class, new()
    {
        if (stream == null || stream.Length == 0)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ExcelContentIsEmpty);
        }

        var rows = sheetName.IsNullOrWhiteSpace()
            ? MiniExcel.Query<T>(stream).ToList()
            : MiniExcel.Query<T>(stream, sheetName: sheetName).ToList();

        // 全空行会被解析成默认值对象，这里统一剔除
        var result = rows.Where(x => x != null).ToList();

        if (result.Count == 0)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ExcelContentIsEmpty);
        }

        if (result.Count > ExcelConsts.MaxImportRows)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ExcelRowsExceeded)
                .WithData("MaxRows", ExcelConsts.MaxImportRows);
        }

        return Task.FromResult(result);
    }
}
