using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.OperationLogs;

namespace Xhj.Project.EntityFrameworkCore.OperationLogs;

/// <summary>
/// 操作日志实体的 EF Core 映射。
/// </summary>
public class OperationLogConfiguration : IEntityTypeConfiguration<OperationLog>
{
    /// <summary>
    /// 配置操作日志表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<OperationLog> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "OperationLogs", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Module).IsRequired().HasMaxLength(OperationLogConsts.MaxModuleLength);
        b.Property(x => x.Operation).IsRequired().HasMaxLength(OperationLogConsts.MaxOperationLength);
        b.Property(x => x.Description).HasMaxLength(OperationLogConsts.MaxDescriptionLength);
        b.Property(x => x.EntityType).HasMaxLength(OperationLogConsts.MaxEntityTypeLength);
        b.Property(x => x.EntityId).HasMaxLength(OperationLogConsts.MaxEntityIdLength);
        b.Property(x => x.UserName).HasMaxLength(OperationLogConsts.MaxUserNameLength);
        b.Property(x => x.ClientIpAddress).HasMaxLength(OperationLogConsts.MaxClientIpAddressLength);
        b.Property(x => x.UserAgent).HasMaxLength(OperationLogConsts.MaxUserAgentLength);
        b.Property(x => x.ErrorMessage).HasMaxLength(OperationLogConsts.MaxErrorMessageLength);

        // 查询通常按时间倒序 + 按模块/操作人筛选
        b.HasIndex(x => x.Module);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.CreationTime);
    }
}
