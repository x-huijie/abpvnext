namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息字段的长度约束，实体、DTO、EF 映射共用一份。
/// </summary>
public static class NotificationConsts
{
    /// <summary>
    /// 消息标题最大长度。
    /// </summary>
    public const int MaxTitleLength = 256;

    /// <summary>
    /// 消息正文最大长度。
    /// </summary>
    public const int MaxContentLength = 4000;

    /// <summary>
    /// 业务来源标识最大长度，如 "Order:123"，便于前端跳转。
    /// </summary>
    public const int MaxSourceKeyLength = 128;
}
