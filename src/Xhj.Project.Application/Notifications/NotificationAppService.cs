using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Xhj.Project.Permissions;

namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息应用服务：发送、我的消息列表、已读标记与删除。
/// </summary>
/// <remarks>
/// 消息内容（<see cref="Notification"/>）与收件箱（<see cref="UserNotification"/>）分离：
/// 查询"我的消息"时两者联表，已读状态按人独立维护。
/// </remarks>
[Authorize(ProjectPermissions.Notifications.Default)]
public class NotificationAppService : ProjectAppService, INotificationAppService
{
    private readonly NotificationManager _notificationManager;
    private readonly IRepository<Notification, Guid> _notificationRepository;
    private readonly IRepository<UserNotification, Guid> _userNotificationRepository;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 构造站内消息应用服务。
    /// </summary>
    /// <param name="notificationManager">消息领域服务。</param>
    /// <param name="notificationRepository">消息仓储。</param>
    /// <param name="userNotificationRepository">收件箱仓储。</param>
    /// <param name="currentUser">当前用户。</param>
    public NotificationAppService(
        NotificationManager notificationManager,
        IRepository<Notification, Guid> notificationRepository,
        IRepository<UserNotification, Guid> userNotificationRepository,
        ICurrentUser currentUser)
    {
        _notificationManager = notificationManager;
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 发送消息给指定用户。
    /// </summary>
    /// <param name="input">消息内容与接收人。</param>
    /// <returns>消息 Id。</returns>
    [Authorize(ProjectPermissions.Notifications.Send)]
    public virtual async Task<Guid> SendAsync(SendNotificationInput input)
    {
        var notification = await _notificationManager.SendAsync(
            input.Title,
            input.Content,
            input.Type,
            input.Severity,
            input.SourceKey,
            _currentUser.Id,
            input.UserIds);

        return notification.Id;
    }

    /// <summary>
    /// 查询当前登录用户的消息列表。
    /// </summary>
    /// <param name="input">过滤与分页条件。</param>
    /// <returns>消息分页结果。</returns>
    public virtual async Task<PagedResultDto<UserNotificationDto>> GetMyListAsync(GetMyNotificationListInput input)
    {
        var userId = GetRequiredUserId();

        var userNotificationQuery = await _userNotificationRepository.GetQueryableAsync();
        var notificationQuery = await _notificationRepository.GetQueryableAsync();

        var query = from userNotification in userNotificationQuery
            join notification in notificationQuery on userNotification.NotificationId equals notification.Id
            where userNotification.UserId == userId
            select new { UserNotification = userNotification, Notification = notification };

        if (input.IsRead.HasValue)
        {
            query = query.Where(x => x.UserNotification.IsRead == input.IsRead.Value);
        }

        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Notification.Type == input.Type.Value);
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Notification.Title.Contains(input.Filter!));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(x => x.UserNotification.IsRead)
                .ThenByDescending(x => x.UserNotification.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var dtos = items
            .Select(x => BuildDto(x.UserNotification, x.Notification))
            .ToList();

        return new PagedResultDto<UserNotificationDto>(totalCount, dtos);
    }

    /// <summary>
    /// 查询当前登录用户的未读消息数量。
    /// </summary>
    /// <returns>未读条数。</returns>
    public virtual async Task<int> GetMyUnreadCountAsync()
    {
        var userId = GetRequiredUserId();

        var query = await _userNotificationRepository.GetQueryableAsync();

        return await AsyncExecuter.CountAsync(query.Where(x => x.UserId == userId && !x.IsRead));
    }

    /// <summary>
    /// 把指定消息标记为已读。
    /// </summary>
    /// <param name="id">收件箱记录 Id。</param>
    /// <exception cref="BusinessException">记录不存在或不属于当前用户时抛出。</exception>
    public virtual async Task MarkAsReadAsync(Guid id)
    {
        var userId = GetRequiredUserId();

        var userNotification = await _userNotificationRepository.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == userId);

        if (userNotification == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.UserNotificationNotFound).WithData("Id", id);
        }

        userNotification.MarkAsRead(DateTime.UtcNow);

        await _userNotificationRepository.UpdateAsync(userNotification, autoSave: true);
    }

    /// <summary>
    /// 把当前用户的全部消息标记为已读。
    /// </summary>
    /// <returns>本次被标记为已读的条数。</returns>
    public virtual async Task<int> MarkAllAsReadAsync()
    {
        var userId = GetRequiredUserId();

        var query = await _userNotificationRepository.GetQueryableAsync();
        var unreadList = await AsyncExecuter.ToListAsync(
            query.Where(x => x.UserId == userId && !x.IsRead));

        var readTime = DateTime.UtcNow;

        foreach (var userNotification in unreadList)
        {
            userNotification.MarkAsRead(readTime);
        }

        if (unreadList.Count > 0)
        {
            await _userNotificationRepository.UpdateManyAsync(unreadList, autoSave: true);
        }

        return unreadList.Count;
    }

    /// <summary>
    /// 删除当前用户的一条消息（只删自己的收件箱记录）。
    /// </summary>
    /// <param name="id">收件箱记录 Id。</param>
    public virtual async Task DeleteAsync(Guid id)
    {
        var userId = GetRequiredUserId();

        var userNotification = await _userNotificationRepository.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == userId);

        // 只影响自己的收件箱，幂等处理避免重复删除报错
        if (userNotification == null)
        {
            return;
        }

        await _userNotificationRepository.DeleteAsync(userNotification, autoSave: true);
    }

    /// <summary>
    /// 组装收件箱 DTO：收件箱状态 + 消息正文。
    /// </summary>
    /// <param name="userNotification">收件箱记录。</param>
    /// <param name="notification">消息内容。</param>
    /// <returns>收件箱 DTO。</returns>
    private static UserNotificationDto BuildDto(UserNotification userNotification, Notification notification)
    {
        return new UserNotificationDto
        {
            Id = userNotification.Id,
            CreationTime = userNotification.CreationTime,
            CreatorId = userNotification.CreatorId,
            TenantId = userNotification.TenantId,
            NotificationId = notification.Id,
            Title = notification.Title,
            Content = notification.Content,
            Type = notification.Type,
            Severity = notification.Severity,
            SourceKey = notification.SourceKey,
            IsRead = userNotification.IsRead,
            ReadTime = userNotification.ReadTime
        };
    }

    /// <summary>
    /// 取当前用户 Id；未登录时抛业务异常。
    /// </summary>
    /// <returns>当前用户 Id。</returns>
    /// <exception cref="BusinessException">未登录时抛出。</exception>
    private Guid GetRequiredUserId()
    {
        return _currentUser.Id ?? throw new BusinessException(ProjectDomainErrorCodes.UserNotificationNotFound)
            .WithData("Message", "当前用户未登录。");
    }
}
