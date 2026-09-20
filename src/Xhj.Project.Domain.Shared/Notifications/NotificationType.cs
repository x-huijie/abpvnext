namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息的分类。
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 系统消息，如维护公告、版本升级。
    /// </summary>
    System = 1,

    /// <summary>
    /// 业务消息，如订单审核结果、任务分派。
    /// </summary>
    Business = 2,

    /// <summary>
    /// 提醒消息，如待办提醒、超时提醒。
    /// </summary>
    Remind = 3
}
