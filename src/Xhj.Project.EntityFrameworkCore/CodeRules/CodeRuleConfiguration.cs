using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.CodeRules;

namespace Xhj.Project.EntityFrameworkCore.CodeRules;

/// <summary>
/// 编码规则实体的 EF Core 映射。
/// </summary>
public class CodeRuleConfiguration : IEntityTypeConfiguration<CodeRule>
{
    /// <summary>
    /// 配置编码规则表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<CodeRule> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "CodeRules", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Code).IsRequired().HasMaxLength(CodeRuleConsts.MaxCodeLength);
        b.Property(x => x.Name).IsRequired().HasMaxLength(CodeRuleConsts.MaxNameLength);
        b.Property(x => x.Prefix).HasMaxLength(CodeRuleConsts.MaxPrefixLength);
        b.Property(x => x.DateFormat).HasMaxLength(CodeRuleConsts.MaxDateFormatLength);
        b.Property(x => x.Separator).HasMaxLength(CodeRuleConsts.MaxSeparatorLength);
        b.Property(x => x.LastResetKey).HasMaxLength(32);

        // 生成单号时按规则标识定位规则，是唯一的查询入口
        b.HasIndex(x => x.Code);
    }
}
