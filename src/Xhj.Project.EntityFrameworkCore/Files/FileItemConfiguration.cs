using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Xhj.Project.Files;

namespace Xhj.Project.EntityFrameworkCore.Files;

/// <summary>
/// 文件记录实体的 EF Core 映射。
/// </summary>
public class FileItemConfiguration : IEntityTypeConfiguration<FileItem>
{
    /// <summary>
    /// 配置文件记录表。
    /// </summary>
    /// <param name="b">实体类型构造器。</param>
    public void Configure(EntityTypeBuilder<FileItem> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "Files", ProjectConsts.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.FileName).IsRequired().HasMaxLength(FileConsts.MaxFileNameLength);
        b.Property(x => x.BlobName).IsRequired().HasMaxLength(FileConsts.MaxBlobNameLength);
        b.Property(x => x.ContentType).HasMaxLength(FileConsts.MaxContentTypeLength);
        b.Property(x => x.Directory).HasMaxLength(FileConsts.MaxDirectoryLength);

        // 列表常按虚拟目录分组查询
        b.HasIndex(x => x.Directory);
    }
}
