using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息领域服务：负责创建消息正文并投递到各接收人的收件箱。
/// </summary>
/// <remarks>
/// 发送即"写收件箱"是同步的：用户量可控时实现最简单、读取最快；
/// 若将来用户量很大，可改为广播消息 + 懒创建收件箱（用户首次读取时再生成）。
/// </remarks>
public class NotificationManager : DomainService
{
    private readonly IRepository<Notification, Guid> _notificationRepository;
    private readonly IRepository<UserNotification, Guid> _userNotificationRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// 构造消息领域服务。
    /// </summary>
    /// <param name="notificationRepository">消息仓储。</param>
    /// <param name="userNotificationRepository">收件箱仓储。</param>
    /// <param name="unitOfWorkManager">工作单元管理器，用于批量提交收件箱。</param>
    public NotificationManager(
        IRepository<Notification, Guid> notificationRepository,
        IRepository<UserNotification, Guid> userNotificationRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 创建消息并投递给指定用户（已持久化）。
    /// </summary>
    /// <param name="title">标题。</param>
    /// <param name="content">正文，可为空。</param>
    /// <param name="type">消息分类。</param>
    /// <param name="severity">重要程度。</param>
    /// <param name="sourceKey">业务来源标识，可为空。</param>
    /// <param name="senderId">发送人 Id，可为空。</param>
    /// <param name="userIds">接收人 Id 集合。</param>
    /// <returns>创建的消息。</returns>
    /// <exception cref="BusinessException">接收人为空时抛出。</exception>
    public virtual async Task<Notification> SendAsync(
        string title,
        string? content,
        NotificationType type,
        NotificationSeverity severity,
        string? sourceKey,
        Guid? senderId,
        List<Guid> userIds)
    {
        if (userIds == null || userIds.Count == 0)
        {
            throw new BusinessException(ProjectDomainErrorCodes.UserNotificationNotFound)
                .WithData("Message", "请至少指定一个接收人。");
        }

        var notification = new Notification(
            GuidGenerator.Create(),
            title,
            content,
            type,
            severity,
            sourceKey,
            senderId);

        await _notificationRepository.InsertAsync(notification, autoSave: true);

        foreach (var userId in userIds)
        {
            await _userNotificationRepository.InsertAsync(
                new UserNotification(GuidGenerator.Create(), notification.Id, userId),
                autoSave: false);
        }

        // 收件箱统一在一次提交中落库，避免每个接收人都往返一次数据库
        // 调用方（应用服务）自带工作单元，此处 Current 必然存在
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return notification;
    }
}
