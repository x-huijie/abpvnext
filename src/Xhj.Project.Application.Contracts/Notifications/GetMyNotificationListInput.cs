using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Notifications;

/// <summary>
/// 查询"我的消息"的入参。
/// </summary>
public class GetMyNotificationListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 是否已读；为 null 表示全部。
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// 消息分类；为 null 表示全部。
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// 标题关键字，模糊匹配；为空表示不过滤。
    /// </summary>
    public string? Filter { get; set; }
}
