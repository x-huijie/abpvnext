namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息的重要程度，前端据此决定展示样式。
/// </summary>
public enum NotificationSeverity
{
    /// <summary>
    /// 一般信息。
    /// </summary>
    Info = 1,

    /// <summary>
    /// 成功提示。
    /// </summary>
    Success = 2,

    /// <summary>
    /// 警告。
    /// </summary>
    Warning = 3,

    /// <summary>
    /// 错误/异常。
    /// </summary>
    Error = 4
}
