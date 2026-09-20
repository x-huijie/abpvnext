using System.Collections.Generic;

namespace Xhj.Project.Files;

/// <summary>
/// 文件上传相关的可配置项，由 HttpApi.Host 从配置节绑定。
/// </summary>
public class FileUploadOptions
{
    /// <summary>
    /// 单个文件允许的最大字节数；小于等于 0 表示不限制。
    /// </summary>
    public long MaxFileSize { get; set; } = FileConsts.DefaultMaxFileSize;

    /// <summary>
    /// 允许的扩展名白名单（含点号的小写形式）；空集合表示不限制。
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new()
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx",
        ".zip", ".rar", ".txt"
    };

    /// <summary>
    /// 本地文件存放根路径（相对 Host 内容根目录），切换存储 Provider 后该值可弃用。
    /// </summary>
    public string BasePath { get; set; } = FileConsts.DefaultBasePath;
}
