namespace Xhj.Project.Departments;

/// <summary>
/// 部门（组织机构）字段的长度约束，实体、DTO、EF 映射共用一份，避免三处不一致。
/// </summary>
public static class DepartmentConsts
{
    /// <summary>
    /// 部门名称最大长度。
    /// </summary>
    public const int MaxNameLength = 128;

    /// <summary>
    /// 部门编码最大长度。
    /// </summary>
    public const int MaxCodeLength = 64;

    /// <summary>
    /// 部门负责人姓名最大长度。
    /// </summary>
    public const int MaxLeaderLength = 64;

    /// <summary>
    /// 部门联系电话最大长度。
    /// </summary>
    public const int MaxPhoneNumberLength = 32;

    /// <summary>
    /// 部门备注最大长度。
    /// </summary>
    public const int MaxRemarkLength = 512;
}
