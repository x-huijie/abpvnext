using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Xhj.Project.Files;

/// <summary>
/// 文件领域服务：负责文件校验（大小、扩展名）与 Blob 对象名生成。
/// </summary>
/// <remarks>
/// 只做校验与命名，不触碰 HTTP 与存储实现，具体的读写由应用服务通过 Blob 容器完成。
/// </remarks>
public class FileManager : DomainService
{
    private readonly FileUploadOptions _options;

    /// <summary>
    /// 构造文件领域服务。
    /// </summary>
    /// <param name="options">上传配置（大小上限、白名单、根路径）。</param>
    public FileManager(IOptions<FileUploadOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 校验文件是否允许上传：先校验扩展名，再校验大小。
    /// </summary>
    /// <param name="fileName">原始文件名。</param>
    /// <param name="size">文件字节数。</param>
    /// <exception cref="BusinessException">类型不允许或大小超限时抛出。</exception>
    public virtual void EnsureFileIsValid(string fileName, long size)
    {
        EnsureExtensionIsAllowed(fileName);
        EnsureSizeIsAllowed(size);
    }

    /// <summary>
    /// 校验扩展名是否在白名单内。
    /// </summary>
    /// <param name="fileName">原始文件名。</param>
    /// <exception cref="BusinessException">扩展名不在白名单时抛出。</exception>
    public virtual void EnsureExtensionIsAllowed(string fileName)
    {
        // 白名单为空表示不限制
        if (_options.AllowedExtensions == null || _options.AllowedExtensions.Count == 0)
        {
            return;
        }

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

        if (extension.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileExtensionNotAllowed);
        }

        if (!_options.AllowedExtensions.Contains(extension!))
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileExtensionNotAllowed)
                .WithData("Extension", extension);
        }
    }

    /// <summary>
    /// 校验文件大小是否超过上限。
    /// </summary>
    /// <param name="size">文件字节数。</param>
    /// <exception cref="BusinessException">超过配置上限时抛出。</exception>
    public virtual void EnsureSizeIsAllowed(long size)
    {
        if (_options.MaxFileSize <= 0)
        {
            return;
        }

        if (size > _options.MaxFileSize)
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileSizeExceeded)
                .WithData("MaxSize", _options.MaxFileSize);
        }
    }

    /// <summary>
    /// 生成本次上传使用的 Blob 对象名。
    /// </summary>
    /// <param name="fileName">原始文件名，用于取扩展名。</param>
    /// <param name="directory">虚拟目录，可为空。</param>
    /// <returns>形如 "2026/09/{guid}.png" 的对象名。</returns>
    /// <remarks>
    /// 按年月分目录可以避免单目录文件过多；用 Guid 命名可避免重名覆盖与路径穿越风险。
    /// </remarks>
    public virtual string BuildBlobName(string fileName, string? directory)
    {
        var extension = Path.GetExtension(fileName);
        var now = DateTime.Now;

        var relativeName = $"{now:yyyy}/{now:MM}/{Guid.NewGuid():N}{extension}";

        return directory.IsNullOrWhiteSpace()
            ? relativeName
            : $"{directory!.Trim('/')}/{relativeName}";
    }
}
