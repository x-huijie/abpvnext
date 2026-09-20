using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Notifications;

/// <summary>
/// 用户收件箱聚合根：描述"某个用户收到了某条消息"以及已读状态。
/// </summary>
/// <remarks>
/// 每个接收者一条记录；删除自己的收件箱记录不会影响消息本身，也不影响其他人。
/// </remarks>
public class UserNotification : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 消息 Id。
    /// </summary>
    public virtual Guid NotificationId { get; protected set; }

    /// <summary>
    /// 接收人 Id。
    /// </summary>
    public virtual Guid UserId { get; protected set; }

    /// <summary>
    /// 是否已读。
    /// </summary>
    public virtual bool IsRead { get; protected set; }

    /// <summary>
    /// 已读时间；未读时为 null。
    /// </summary>
    public virtual DateTime? ReadTime { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected UserNotification()
    {
    }

    /// <summary>
    /// 创建一条收件箱记录。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="notificationId">消息 Id。</param>
    /// <param name="userId">接收人 Id。</param>
    public UserNotification(Guid id, Guid notificationId, Guid userId)
        : base(id)
    {
        NotificationId = notificationId;
        UserId = userId;
        IsRead = false;
    }

    /// <summary>
    /// 标记为已读；已读状态下重复调用不覆盖首次已读时间。
    /// </summary>
    /// <param name="readTime">已读时间（UTC）。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual UserNotification MarkAsRead(DateTime readTime)
    {
        // 保留首次已读时间，避免重复标记导致时间被刷新
        if (!IsRead)
        {
            IsRead = true;
            ReadTime = readTime;
        }

        return this;
    }
}
