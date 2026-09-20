using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.Notifications;

namespace Xhj.Project.EntityFrameworkCore.Notifications;

/// <summary>
/// 用户收件箱实体的 EF Core 映射。
/// </summary>
public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    /// <summary>
    /// 配置收件箱表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<UserNotification> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "UserNotifications", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        // 未读数与消息列表都按"用户 + 已读状态"查询，是最主要的查询路径
        b.HasIndex(x => new { x.UserId, x.IsRead });
        b.HasIndex(x => x.NotificationId);
    }
}
