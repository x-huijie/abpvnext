using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志写入器实现。
/// </summary>
/// <remarks>
/// 两点关键设计：
/// 1. 使用独立的新工作单元（requiresNew）写入，保证主流程回滚时日志仍然留存；
/// 2. 写入异常只记录日志不向上抛，避免"记日志"本身拖垮业务操作。
/// </remarks>
public class OperationLogWriter : IOperationLogWriter, ITransientDependency
{
    private readonly IRepository<OperationLog, Guid> _operationLogRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRequestInfoProvider? _requestInfoProvider;
    private readonly ICurrentUser _currentUser;
    private readonly IGuidGenerator _guidGenerator;

    /// <summary>
    /// 属性注入的日志器。
    /// </summary>
    public ILogger<OperationLogWriter> Logger { get; set; }

    /// <summary>
    /// 构造操作日志写入器。
    /// </summary>
    /// <param name="operationLogRepository">操作日志仓储。</param>
    /// <param name="unitOfWorkManager">工作单元管理器。</param>
    /// <param name="currentUser">当前用户。</param>
    /// <param name="guidGenerator">Id 生成器。</param>
    /// <param name="requestInfoProvider">请求环境信息提供者，可为空（后台任务场景）。</param>
    public OperationLogWriter(
        IRepository<OperationLog, Guid> operationLogRepository,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentUser currentUser,
        IGuidGenerator guidGenerator,
        IRequestInfoProvider? requestInfoProvider = null)
    {
        _operationLogRepository = operationLogRepository;
        _unitOfWorkManager = unitOfWorkManager;
        _currentUser = currentUser;
        _guidGenerator = guidGenerator;
        _requestInfoProvider = requestInfoProvider;

        Logger = NullLogger<OperationLogWriter>.Instance;
    }

    /// <summary>
    /// 记录一条操作日志。
    /// </summary>
    /// <param name="input">操作信息。</param>
    /// <returns>异步任务。</returns>
    public virtual async Task WriteAsync(WriteOperationLogInput input)
    {
        try
        {
            // 独立工作单元：主流程事务回滚时，操作日志仍然保留，便于追溯失败操作
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);

            var log = new OperationLog(
                _guidGenerator.Create(),
                input.Module,
                input.Operation,
                input.OperationType,
                input.Description,
                input.EntityType,
                input.EntityId);

            log.SetUser(_currentUser.Id, _currentUser.UserName)
                .SetExecutionDuration(input.ExecutionDuration);

            if (_requestInfoProvider != null)
            {
                log.SetClientInfo(
                    _requestInfoProvider.GetClientIpAddress(),
                    _requestInfoProvider.GetUserAgent());
            }

            if (!input.IsSuccess)
            {
                log.MarkAsFailed(input.ErrorMessage);
            }

            await _operationLogRepository.InsertAsync(log, autoSave: true);
            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            // 记录日志失败不能影响业务主流程，只输出到日志系统
            Logger.LogWarning(ex, "写入操作日志失败：{Operation}", input.Operation);
        }
    }
}
