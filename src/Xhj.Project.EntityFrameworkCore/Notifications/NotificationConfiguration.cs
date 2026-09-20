using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.Notifications;

namespace Xhj.Project.EntityFrameworkCore.Notifications;

/// <summary>
/// 站内消息实体的 EF Core 映射。
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <summary>
    /// 配置消息表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "Notifications", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Title).IsRequired().HasMaxLength(NotificationConsts.MaxTitleLength);
        b.Property(x => x.Content).HasMaxLength(NotificationConsts.MaxContentLength);
        b.Property(x => x.SourceKey).HasMaxLength(NotificationConsts.MaxSourceKeyLength);

        // 按业务来源反查消息，用于"同一业务只推一条"的去重
        b.HasIndex(x => x.SourceKey);
    }
}
