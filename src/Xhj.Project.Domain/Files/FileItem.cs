using System;
using System.IO;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Files;

/// <summary>
/// 文件记录聚合根：只保存文件的元数据，真实内容交由 ABP Blob 容器存储。
/// </summary>
/// <remarks>
/// 这样设计的好处是：换存储（本地/OSS/MinIO）时领域模型与业务代码都不用改，
/// 只需在 Host 层替换 Blob Provider 的配置。
/// </remarks>
public class FileItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 原始文件名（含扩展名）。
    /// </summary>
    public virtual string FileName { get; protected set; } = default!;

    /// <summary>
    /// 文件的 MIME 类型，可为空。
    /// </summary>
    public virtual string? ContentType { get; protected set; }

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    public virtual long Size { get; protected set; }

    /// <summary>
    /// Blob 容器中的对象名，形如 "2026/09/xxxxxx.png"，是读取内容的唯一依据。
    /// </summary>
    public virtual string BlobName { get; protected set; } = default!;

    /// <summary>
    /// 虚拟目录（业务分组），可为空表示根目录。
    /// </summary>
    public virtual string? Directory { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected FileItem()
    {
        FileName = string.Empty;
        BlobName = string.Empty;
    }

    /// <summary>
    /// 创建文件记录。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="fileName">原始文件名。</param>
    /// <param name="blobName">Blob 对象名。</param>
    /// <param name="size">文件大小（字节）。</param>
    /// <param name="contentType">MIME 类型，可为空。</param>
    /// <param name="directory">虚拟目录，可为空。</param>
    public FileItem(
        Guid id,
        string fileName,
        string blobName,
        long size,
        string? contentType = null,
        string? directory = null)
        : base(id)
    {
        SetFileName(fileName);
        SetBlobName(blobName);
        SetSize(size);
        SetContentType(contentType);
        SetDirectory(directory);
    }

    /// <summary>
    /// 修改文件名，校验必填与长度。
    /// </summary>
    /// <param name="fileName">文件名。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual FileItem SetFileName(string fileName)
    {
        Check.NotNullOrWhiteSpace(fileName, nameof(fileName), FileConsts.MaxFileNameLength);

        FileName = fileName;
        return this;
    }

    /// <summary>
    /// 修改 Blob 对象名，校验必填与长度。
    /// </summary>
    /// <param name="blobName">Blob 对象名。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual FileItem SetBlobName(string blobName)
    {
        Check.NotNullOrWhiteSpace(blobName, nameof(blobName), FileConsts.MaxBlobNameLength);

        BlobName = blobName;
        return this;
    }

    /// <summary>
    /// 修改文件大小，不允许为负数。
    /// </summary>
    /// <param name="size">字节数。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual FileItem SetSize(long size)
    {
        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "文件大小不能为负数。");
        }

        Size = size;
        return this;
    }

    /// <summary>
    /// 修改 MIME 类型。
    /// </summary>
    /// <param name="contentType">MIME 类型，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual FileItem SetContentType(string? contentType)
    {
        ContentType = contentType;
        return this;
    }

    /// <summary>
    /// 修改虚拟目录。
    /// </summary>
    /// <param name="directory">目录名，可为空表示根目录。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual FileItem SetDirectory(string? directory)
    {
        Directory = directory;
        return this;
    }

    /// <summary>
    /// 获取文件扩展名（含点号的小写形式）；无扩展名时返回空字符串。
    /// </summary>
    /// <returns>如 ".png"。</returns>
    public virtual string GetExtension()
    {
        return Path.GetExtension(FileName)?.ToLowerInvariant() ?? string.Empty;
    }
}
