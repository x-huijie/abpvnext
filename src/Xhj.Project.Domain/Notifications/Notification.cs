using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息聚合根：只保存消息内容，接收与已读状态由 <see cref="UserNotification"/> 承载。
/// </summary>
/// <remarks>
/// 内容与收件箱分离的好处：一条广播消息只存一份正文，各用户的已读状态独立维护。
/// </remarks>
public class Notification : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 消息标题。
    /// </summary>
    public virtual string Title { get; protected set; } = default!;

    /// <summary>
    /// 消息正文，可为空。
    /// </summary>
    public virtual string? Content { get; protected set; }

    /// <summary>
    /// 消息分类。
    /// </summary>
    public virtual NotificationType Type { get; protected set; }

    /// <summary>
    /// 重要程度。
    /// </summary>
    public virtual NotificationSeverity Severity { get; protected set; }

    /// <summary>
    /// 业务来源标识，如 "Order:123"，便于前端跳转详情页；可为空。
    /// </summary>
    public virtual string? SourceKey { get; protected set; }

    /// <summary>
    /// 发送人 Id，可为空（系统消息）。
    /// </summary>
    public virtual Guid? SenderId { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected Notification()
    {
        Title = string.Empty;
    }

    /// <summary>
    /// 创建一条站内消息。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="title">标题。</param>
    /// <param name="content">正文，可为空。</param>
    /// <param name="type">消息分类。</param>
    /// <param name="severity">重要程度。</param>
    /// <param name="sourceKey">业务来源标识，可为空。</param>
    /// <param name="senderId">发送人 Id，可为空。</param>
    public Notification(
        Guid id,
        string title,
        string? content,
        NotificationType type = NotificationType.System,
        NotificationSeverity severity = NotificationSeverity.Info,
        string? sourceKey = null,
        Guid? senderId = null)
        : base(id)
    {
        SetTitle(title);
        SetContent(content);
        Type = type;
        Severity = severity;
        SourceKey = sourceKey;
        SenderId = senderId;
    }

    /// <summary>
    /// 修改标题，校验必填与长度。
    /// </summary>
    /// <param name="title">标题。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Notification SetTitle(string title)
    {
        Check.NotNullOrWhiteSpace(title, nameof(title), NotificationConsts.MaxTitleLength);

        Title = title;
        return this;
    }

    /// <summary>
    /// 修改正文。
    /// </summary>
    /// <param name="content">正文，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Notification SetContent(string? content)
    {
        Content = content;
        return this;
    }
}
