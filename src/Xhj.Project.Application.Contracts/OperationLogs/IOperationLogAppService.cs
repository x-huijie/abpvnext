using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志应用服务：提供查询能力（写入能力由 <c>IOperationLogWriter</c> 提供）。
/// </summary>
/// <remarks>
/// 只保留查询：操作日志不允许人工修改，以保证追溯可信。
/// </remarks>
public interface IOperationLogAppService : IApplicationService
{
    /// <summary>
    /// 分页查询操作日志。
    /// </summary>
    /// <param name="input">过滤与分页条件。</param>
    /// <returns>操作日志分页结果。</returns>
    Task<PagedResultDto<OperationLogDto>> GetListAsync(GetOperationLogListInput input);

    /// <summary>
    /// 查询单条操作日志。
    /// </summary>
    /// <param name="id">日志 Id。</param>
    /// <returns>操作日志。</returns>
    Task<OperationLogDto> GetAsync(Guid id);
}
