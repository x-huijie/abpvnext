namespace Xhj.Project.OperationLogs;

/// <summary>
/// 操作日志字段的长度约束，实体、DTO、EF 映射共用一份。
/// </summary>
public static class OperationLogConsts
{
    /// <summary>
    /// 业务模块名最大长度，如 "Departments"。
    /// </summary>
    public const int MaxModuleLength = 64;

    /// <summary>
    /// 操作名称最大长度，如 "删除部门"。
    /// </summary>
    public const int MaxOperationLength = 128;

    /// <summary>
    /// 操作内容描述最大长度。
    /// </summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>
    /// 关联实体类型名最大长度。
    /// </summary>
    public const int MaxEntityTypeLength = 256;

    /// <summary>
    /// 关联实体 Id 最大长度（以字符串存储，兼容非 Guid 主键）。
    /// </summary>
    public const int MaxEntityIdLength = 64;

    /// <summary>
    /// 操作人用户名最大长度。
    /// </summary>
    public const int MaxUserNameLength = 256;

    /// <summary>
    /// 客户端 IP 最大长度。
    /// </summary>
    public const int MaxClientIpAddressLength = 64;

    /// <summary>
    /// 客户端 UserAgent 最大长度。
    /// </summary>
    public const int MaxUserAgentLength = 512;

    /// <summary>
    /// 失败原因最大长度。
    /// </summary>
    public const int MaxErrorMessageLength = 2000;
}
