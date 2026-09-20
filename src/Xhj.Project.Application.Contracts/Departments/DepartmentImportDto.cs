namespace Xhj.Project.Departments;

/// <summary>
/// 部门 Excel 导入的行模型，属性名需与模板表头一致。
/// </summary>
/// <remarks>
/// <see cref="ParentCode"/> 使用上级部门的<b>编码</b>而不是 Id，便于人工填写模板；
/// 导入时由应用服务把它转换为 ParentId。
/// </remarks>
public class DepartmentImportDto
{
    /// <summary>
    /// 部门名称，必填。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 部门编码，必填且租户内唯一。
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 上级部门编码，可为空表示一级部门。
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// 部门负责人，可为空。
    /// </summary>
    public string? Leader { get; set; }

    /// <summary>
    /// 联系电话，可为空。
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 排序号。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用，默认 true。
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 备注，可为空。
    /// </summary>
    public string? Remark { get; set; }
}
