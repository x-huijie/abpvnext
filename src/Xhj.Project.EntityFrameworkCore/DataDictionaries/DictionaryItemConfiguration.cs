using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.DataDictionaries;

namespace Xhj.Project.EntityFrameworkCore.DataDictionaries;

/// <summary>
/// 字典项实体的 EF Core 映射。
/// </summary>
public class DictionaryItemConfiguration : IEntityTypeConfiguration<DictionaryItem>
{
    /// <summary>
    /// 配置字典项表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<DictionaryItem> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "DictionaryItems", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Label).IsRequired().HasMaxLength(DataDictionaryConsts.MaxLabelLength);
        b.Property(x => x.Value).IsRequired().HasMaxLength(DataDictionaryConsts.MaxValueLength);
        b.Property(x => x.Description).HasMaxLength(DataDictionaryConsts.MaxDescriptionLength);

        // 高频查询：按类型 + 启用状态检索，联合索引覆盖该查询路径
        b.HasIndex(x => new { x.DictionaryTypeId, x.IsEnabled });
    }
}
