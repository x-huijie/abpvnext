using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Xhj.Project.Notifications;

/// <summary>
/// 站内消息应用服务：发送、我的消息列表、已读标记与删除。
/// </summary>
public interface INotificationAppService : IApplicationService
{
    /// <summary>
    /// 发送消息给指定用户。
    /// </summary>
    /// <param name="input">消息内容与接收人。</param>
    /// <returns>消息 Id。</returns>
    Task<Guid> SendAsync(SendNotificationInput input);

    /// <summary>
    /// 查询当前登录用户的消息列表。
    /// </summary>
    /// <param name="input">过滤与分页条件。</param>
    /// <returns>消息分页结果。</returns>
    Task<PagedResultDto<UserNotificationDto>> GetMyListAsync(GetMyNotificationListInput input);

    /// <summary>
    /// 查询当前登录用户的未读消息数量。
    /// </summary>
    /// <returns>未读条数。</returns>
    Task<int> GetMyUnreadCountAsync();

    /// <summary>
    /// 把指定消息标记为已读。
    /// </summary>
    /// <param name="id">收件箱记录 Id。</param>
    Task MarkAsReadAsync(Guid id);

    /// <summary>
    /// 把当前用户的全部消息标记为已读。
    /// </summary>
    /// <returns>本次被标记为已读的条数。</returns>
    Task<int> MarkAllAsReadAsync();

    /// <summary>
    /// 删除当前用户的一条消息（只删自己的收件箱记录）。
    /// </summary>
    /// <param name="id">收件箱记录 Id。</param>
    Task DeleteAsync(Guid id);
}
