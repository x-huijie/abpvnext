using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.Files;

/// <summary>
/// 文件应用服务：上传、下载、查询与删除。
/// </summary>
/// <remarks>
/// 文件内容统一写入 ABP Blob 容器 <see cref="ProjectFileBlobContainer"/>；
/// 当前 Host 侧配置为本地文件系统，将来切换到 OSS/MinIO 只需替换 Provider 配置。
/// </remarks>
[Authorize(ProjectPermissions.Files.Default)]
public class FileAppService : ProjectAppService, IFileAppService
{
    private readonly IRepository<FileItem, Guid> _fileRepository;
    private readonly FileManager _fileManager;
    private readonly IBlobContainer<ProjectFileBlobContainer> _blobContainer;

    /// <summary>
    /// 构造文件应用服务。
    /// </summary>
    /// <param name="fileRepository">文件记录仓储。</param>
    /// <param name="fileManager">文件领域服务（校验与命名）。</param>
    /// <param name="blobContainer">文件 Blob 容器。</param>
    public FileAppService(
        IRepository<FileItem, Guid> fileRepository,
        FileManager fileManager,
        IBlobContainer<ProjectFileBlobContainer> blobContainer)
    {
        _fileRepository = fileRepository;
        _fileManager = fileManager;
        _blobContainer = blobContainer;
    }

    /// <summary>
    /// 上传单个文件。
    /// </summary>
    /// <param name="file">文件内容流。</param>
    /// <param name="directory">虚拟目录，可为空表示根目录。</param>
    /// <returns>文件记录。</returns>
    /// <exception cref="BusinessException">文件类型不允许或大小超限时抛出。</exception>
    [Authorize(ProjectPermissions.Files.Upload)]
    public virtual async Task<FileItemDto> UploadAsync(IRemoteStreamContent file, string? directory = null)
    {
        if (file == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileNotFound);
        }

        var fileName = string.IsNullOrWhiteSpace(file.FileName) ? "unnamed" : file.FileName!;

        // 先读入内存才能拿到准确大小并做校验，默认限制在几 MB 以内
        using var memoryStream = new MemoryStream();
        await file.GetStream().CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        _fileManager.EnsureFileIsValid(fileName, bytes.Length);

        var blobName = _fileManager.BuildBlobName(fileName, directory);

        await _blobContainer.SaveAsync(blobName, bytes, overrideExisting: true);

        var fileItem = new FileItem(
            GuidGenerator.Create(),
            fileName,
            blobName,
            bytes.Length,
            file.ContentType,
            directory);

        await _fileRepository.InsertAsync(fileItem, autoSave: true);

        return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
    }

    /// <summary>
    /// 下载文件内容。
    /// </summary>
    /// <param name="id">文件记录 Id。</param>
    /// <returns>可下载的内容流。</returns>
    /// <exception cref="BusinessException">文件记录不存在，或存储内容已丢失时抛出。</exception>
    public virtual async Task<IRemoteStreamContent> DownloadAsync(Guid id)
    {
        var fileItem = await _fileRepository.FindAsync(id);

        if (fileItem == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileNotFound).WithData("Id", id);
        }

        var bytes = await _blobContainer.GetAllBytesOrNullAsync(fileItem.BlobName);

        if (bytes == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileNotFound)
                .WithData("BlobName", fileItem.BlobName);
        }

        return new RemoteStreamContent(new MemoryStream(bytes), fileItem.FileName, fileItem.ContentType);
    }

    /// <summary>
    /// 分页查询文件列表。
    /// </summary>
    /// <param name="input">过滤与分页条件。</param>
    /// <returns>文件记录分页结果。</returns>
    public virtual async Task<PagedResultDto<FileItemDto>> GetListAsync(GetFileListInput input)
    {
        var query = await _fileRepository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.FileName.Contains(input.Filter!));
        }

        if (!input.Directory.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Directory == input.Directory);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<FileItemDto>(
            totalCount,
            items.Select(x => ObjectMapper.Map<FileItem, FileItemDto>(x)).ToList());
    }

    /// <summary>
    /// 删除文件记录及其存储内容。
    /// </summary>
    /// <param name="id">文件记录 Id。</param>
    /// <exception cref="BusinessException">文件记录不存在时抛出。</exception>
    [Authorize(ProjectPermissions.Files.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var fileItem = await _fileRepository.FindAsync(id);

        if (fileItem == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.FileNotFound).WithData("Id", id);
        }

        // 先删存储内容再删记录，避免产生孤儿文件
        await _blobContainer.DeleteAsync(fileItem.BlobName);
        await _fileRepository.DeleteAsync(fileItem, autoSave: true);
    }
}
