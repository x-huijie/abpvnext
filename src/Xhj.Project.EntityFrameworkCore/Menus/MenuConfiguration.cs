using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.Menus;

namespace Xhj.Project.EntityFrameworkCore.Menus;

/// <summary>
/// 菜单实体的 EF Core 映射：表名、字段长度与索引。
/// </summary>
public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    /// <summary>
    /// 配置菜单表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<Menu> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "Menus", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(MenuConsts.MaxNameLength);
        b.Property(x => x.DisplayName).IsRequired().HasMaxLength(MenuConsts.MaxDisplayNameLength);
        b.Property(x => x.Path).HasMaxLength(MenuConsts.MaxPathLength);
        b.Property(x => x.Component).HasMaxLength(MenuConsts.MaxComponentLength);
        b.Property(x => x.Icon).HasMaxLength(MenuConsts.MaxIconLength);
        b.Property(x => x.Permission).HasMaxLength(MenuConsts.MaxPermissionLength);
        b.Property(x => x.Remark).HasMaxLength(MenuConsts.MaxRemarkLength);

        // 菜单标识用于唯一性校验、ParentId 用于树查询，均需索引
        b.HasIndex(x => x.Name);
        b.HasIndex(x => x.ParentId);
    }
}
