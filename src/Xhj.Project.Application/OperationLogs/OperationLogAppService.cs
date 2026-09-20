using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志应用服务：只提供查询，不允许人工修改以保证追溯可信。
/// </summary>
[Authorize(ProjectPermissions.OperationLogs.Default)]
public class OperationLogAppService : ProjectAppService, IOperationLogAppService
{
    private readonly IRepository<OperationLog, Guid> _operationLogRepository;

    /// <summary>
    /// 构造操作日志应用服务。
    /// </summary>
    /// <param name="operationLogRepository">操作日志仓储。</param>
    public OperationLogAppService(IRepository<OperationLog, Guid> operationLogRepository)
    {
        _operationLogRepository = operationLogRepository;
    }

    /// <summary>
    /// 分页查询操作日志。
    /// </summary>
    /// <param name="input">过滤与分页条件。</param>
    /// <returns>操作日志分页结果。</returns>
    public virtual async Task<PagedResultDto<OperationLogDto>> GetListAsync(GetOperationLogListInput input)
    {
        var query = await CreateFilteredQueryAsync(input);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<OperationLogDto>(
            totalCount,
            items.Select(x => ObjectMapper.Map<OperationLog, OperationLogDto>(x)).ToList());
    }

    /// <summary>
    /// 查询单条操作日志。
    /// </summary>
    /// <param name="id">日志 Id。</param>
    /// <returns>操作日志。</returns>
    public virtual async Task<OperationLogDto> GetAsync(Guid id)
    {
        var log = await _operationLogRepository.GetAsync(id);

        return ObjectMapper.Map<OperationLog, OperationLogDto>(log);
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    private async Task<IQueryable<OperationLog>> CreateFilteredQueryAsync(GetOperationLogListInput input)
    {
        var query = await _operationLogRepository.GetQueryableAsync();

        if (!input.Module.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Module == input.Module);
        }

        if (input.OperationType.HasValue)
        {
            query = query.Where(x => x.OperationType == input.OperationType.Value);
        }

        if (input.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == input.UserId.Value);
        }

        if (input.IsSuccess.HasValue)
        {
            query = query.Where(x => x.IsSuccess == input.IsSuccess.Value);
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x =>
                x.Operation.Contains(input.Filter!) || (x.Description != null && x.Description.Contains(input.Filter!)));
        }

        if (input.StartTime.HasValue)
        {
            query = query.Where(x => x.CreationTime >= input.StartTime.Value);
        }

        if (input.EndTime.HasValue)
        {
            query = query.Where(x => x.CreationTime <= input.EndTime.Value);
        }

        return query;
    }
}
