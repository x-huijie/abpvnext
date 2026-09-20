using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Notifications;

/// <summary>
/// 发送站内消息的入参。
/// </summary>
public class SendNotificationInput
{
    /// <summary>
    /// 消息标题，必填。
    /// </summary>
    [Required]
    [MaxLength(NotificationConsts.MaxTitleLength)]
    public string Title { get; set; } = default!;

    /// <summary>
    /// 消息正文，可为空。
    /// </summary>
    [MaxLength(NotificationConsts.MaxContentLength)]
    public string? Content { get; set; }

    /// <summary>
    /// 消息分类，默认 <see cref="Notifications.NotificationType.System"/>。
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.System;

    /// <summary>
    /// 重要程度，默认 <see cref="Notifications.NotificationSeverity.Info"/>。
    /// </summary>
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

    /// <summary>
    /// 业务来源标识，如 "Order:123"，可为空。
    /// </summary>
    [MaxLength(NotificationConsts.MaxSourceKeyLength)]
    public string? SourceKey { get; set; }

    /// <summary>
    /// 接收人 Id 集合，必填且至少包含一个用户。
    /// </summary>
    [Required]
    public List<Guid> UserIds { get; set; } = new();
}
