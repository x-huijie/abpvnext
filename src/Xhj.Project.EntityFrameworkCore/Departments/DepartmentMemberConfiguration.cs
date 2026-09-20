using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.Departments;

namespace Xhj.Project.EntityFrameworkCore.Departments;

/// <summary>
/// 部门成员实体的 EF Core 映射。
/// </summary>
public class DepartmentMemberConfiguration : IEntityTypeConfiguration<DepartmentMember>
{
    /// <summary>
    /// 配置部门成员表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<DepartmentMember> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "DepartmentMembers", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        // 数据权限计算时按用户查所属部门，是最主要的查询路径
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => new { x.DepartmentId, x.UserId });
    }
}
