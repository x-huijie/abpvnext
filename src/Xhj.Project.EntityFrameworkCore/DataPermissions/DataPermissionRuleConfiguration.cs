using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.DataPermissions;

namespace Xhj.Project.EntityFrameworkCore.DataPermissions;

/// <summary>
/// 数据权限规则实体的 EF Core 映射。
/// </summary>
public class DataPermissionRuleConfiguration : IEntityTypeConfiguration<DataPermissionRule>
{
    /// <summary>
    /// 配置数据权限规则表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<DataPermissionRule> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "DataPermissionRules", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.RoleName).IsRequired().HasMaxLength(DataPermissionConsts.MaxRoleNameLength);
        b.Property(x => x.ResourceKey).IsRequired().HasMaxLength(DataPermissionConsts.MaxResourceKeyLength);
        b.Property(x => x.DepartmentIds).HasMaxLength(DataPermissionConsts.MaxDepartmentIdsLength);

        // 解析规则时的查询条件是"角色 + 资源"，用联合索引覆盖
        b.HasIndex(x => new { x.RoleId, x.ResourceKey });
    }
}
