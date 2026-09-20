namespace Xhj.Project.Files;

/// <summary>
/// 文件上传相关的字段约束与默认值，实体、DTO、EF 映射共用一份。
/// </summary>
public static class FileConsts
{
    /// <summary>
    /// 原始文件名最大长度。
    /// </summary>
    public const int MaxFileNameLength = 256;

    /// <summary>
    /// 文件ContentType（MIME）最大长度。
    /// </summary>
    public const int MaxContentTypeLength = 128;

    /// <summary>
    /// Blob 存储中的对象名最大长度（形如 "2026/09/xxxxxx.png"）。
    /// </summary>
    public const int MaxBlobNameLength = 512;

    /// <summary>
    /// 存放目录（虚拟目录）最大长度，可为空表示根目录。
    /// </summary>
    public const int MaxDirectoryLength = 128;

    /// <summary>
    /// 默认单文件大小上限（字节），5 MB。
    /// </summary>
    public const long DefaultMaxFileSize = 5 * 1024 * 1024;

    /// <summary>
    /// 默认允许上传的扩展名白名单，逗号分隔；为空表示不限制。
    /// </summary>
    public const string DefaultAllowedExtensions = ".jpg,.jpeg,.png,.gif,.bmp,.pdf,.doc,.docx,.xls,.xlsx,.zip,.rar,.txt";

    /// <summary>
    /// 本地文件存放的根路径（相对 Host 内容根目录）。
    /// </summary>
    public const string DefaultBasePath = "App_Data/uploads";
}
