using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Xhj.Project.Files;

/// <summary>
/// 文件应用服务：上传、下载、查询与删除。
/// </summary>
/// <remarks>
/// 文件内容存放在 ABP Blob 容器中（当前为本地文件系统），后续切换 OSS/MinIO 无需改动接口。
/// </remarks>
public interface IFileAppService : IApplicationService
{
    /// <summary>
    /// 上传单个文件。
    /// </summary>
    /// <param name="file">文件内容流（multipart/form-data）。</param>
    /// <param name="directory">虚拟目录，可为空表示根目录。</param>
    /// <returns>文件记录。</returns>
    Task<FileItemDto> UploadAsync(IRemoteStreamContent file, string? directory = null);

    /// <summary>
    /// 下载文件内容。
    /// </summary>
    /// <param name="id">文件记录 Id。</param>
    /// <returns>可下载的内容流。</returns>
    Task<IRemoteStreamContent> DownloadAsync(Guid id);

    /// <summary>
    /// 分页查询文件列表。
    /// </summary>
    /// <param name="input">过滤与分页条件。</param>
    /// <returns>文件记录分页结果。</returns>
    Task<PagedResultDto<FileItemDto>> GetListAsync(GetFileListInput input);

    /// <summary>
    /// 删除文件记录及其存储内容。
    /// </summary>
    /// <param name="id">文件记录 Id。</param>
    Task DeleteAsync(Guid id);
}
