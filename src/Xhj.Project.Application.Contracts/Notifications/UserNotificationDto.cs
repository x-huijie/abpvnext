using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Notifications;

/// <summary>
/// 用户收件箱输出 DTO：消息内容 + 本人已读状态。
/// </summary>
public class UserNotificationDto : CreationAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 消息 Id。
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 消息标题。
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// 消息正文，可为空。
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 消息分类。
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 重要程度。
    /// </summary>
    public NotificationSeverity Severity { get; set; }

    /// <summary>
    /// 业务来源标识，便于前端跳转，可为空。
    /// </summary>
    public string? SourceKey { get; set; }

    /// <summary>
    /// 是否已读。
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 已读时间，未读时为 null。
    /// </summary>
    public DateTime? ReadTime { get; set; }
}
