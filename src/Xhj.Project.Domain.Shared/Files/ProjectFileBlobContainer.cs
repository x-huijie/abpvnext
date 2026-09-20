namespace Xhj.Project.Files;

/// <summary>
/// ABP Blob 容器的标记类型，用于区分业务文件容器与其他容器。
/// </summary>
/// <remarks>
/// 该类型本身不包含任何成员，仅作为 <c>IBlobContainer&lt;TContainer&gt;</c> 的泛型标记；
/// 不标注 BlobContainerName 时，容器名取该类型的名称，将来切换到 OSS/MinIO 只需替换 Provider 配置。
/// </remarks>
public class ProjectFileBlobContainer
{
}
