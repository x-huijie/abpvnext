using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.Departments;

namespace Xhj.Project.EntityFrameworkCore.Departments;

/// <summary>
/// 部门实体的 EF Core 映射：表名、字段长度与索引。
/// </summary>
/// <remarks>
/// 由 <c>ProjectDbContext.OnModelCreating</c> 中的 ApplyConfigurationsFromAssembly 统一装配。
/// </remarks>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    /// <summary>
    /// 配置部门表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<Department> b)
    {
        // 表名统一带 App 前缀，避免与 ABP 模块表混在一起
        b.ToTable(ProjectConsts.DbTablePrefix + "Departments", ProjectConsts.DbSchema);

        // ConfigureByConvention 负责基类属性（审计、租户、并发戳）与租户过滤
        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(DepartmentConsts.MaxNameLength);
        b.Property(x => x.Code).IsRequired().HasMaxLength(DepartmentConsts.MaxCodeLength);
        b.Property(x => x.Leader).HasMaxLength(DepartmentConsts.MaxLeaderLength);
        b.Property(x => x.PhoneNumber).HasMaxLength(DepartmentConsts.MaxPhoneNumberLength);
        b.Property(x => x.Remark).HasMaxLength(DepartmentConsts.MaxRemarkLength);

        b.HasIndex(x => x.Code);
        b.HasIndex(x => x.ParentId);
    }
}
