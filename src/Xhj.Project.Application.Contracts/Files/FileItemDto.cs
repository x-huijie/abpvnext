using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Files;

/// <summary>
/// 文件记录输出 DTO。
/// </summary>
public class FileItemDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string FileName { get; set; } = default!;

    /// <summary>
    /// MIME 类型，可为空。
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Blob 对象名，业务侧一般无需关心。
    /// </summary>
    public string BlobName { get; set; } = default!;

    /// <summary>
    /// 虚拟目录，可为空表示根目录。
    /// </summary>
    public string? Directory { get; set; }
}
