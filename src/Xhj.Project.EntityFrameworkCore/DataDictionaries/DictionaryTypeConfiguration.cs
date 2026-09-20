using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.DataDictionaries;

namespace Xhj.Project.EntityFrameworkCore.DataDictionaries;

/// <summary>
/// 字典类型实体的 EF Core 映射。
/// </summary>
public class DictionaryTypeConfiguration : IEntityTypeConfiguration<DictionaryType>
{
    /// <summary>
    /// 配置字典类型表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<DictionaryType> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "DictionaryTypes", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Code).IsRequired().HasMaxLength(DataDictionaryConsts.MaxCodeLength);
        b.Property(x => x.DisplayName).IsRequired().HasMaxLength(DataDictionaryConsts.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(DataDictionaryConsts.MaxDescriptionLength);

        // 高频读按编码定位类型，必须建索引
        b.HasIndex(x => x.Code);
    }
}
