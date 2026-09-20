using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门输出 DTO，用于列表与详情返回。
/// </summary>
public class DepartmentDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 部门名称。
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 部门编码，租户内唯一。
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// 上级部门 Id；为 null 表示一级部门。
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 部门负责人姓名，可为空。
    /// </summary>
    public string? Leader { get; set; }

    /// <summary>
    /// 部门联系电话，可为空。
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 同级排序号，值越小越靠前。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 备注说明，可为空。
    /// </summary>
    public string? Remark { get; set; }
}
