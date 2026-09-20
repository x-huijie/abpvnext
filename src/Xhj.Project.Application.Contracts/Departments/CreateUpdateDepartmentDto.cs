using System;
using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门新增/编辑入参，两个操作共用同一模型。
/// </summary>
/// <remarks>
/// 上级部门变更不在本模型中，需调用 <c>PUT /api/app/department/{id}/move</c>，
/// 避免编辑时被误改层级。
/// </remarks>
public class CreateUpdateDepartmentDto
{
    /// <summary>
    /// 部门名称，必填，长度见 <see cref="DepartmentConsts.MaxNameLength"/>。
    /// </summary>
    [Required]
    [MaxLength(DepartmentConsts.MaxNameLength)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 部门编码，必填且租户内唯一，长度见 <see cref="DepartmentConsts.MaxCodeLength"/>。
    /// </summary>
    [Required]
    [MaxLength(DepartmentConsts.MaxCodeLength)]
    public string Code { get; set; } = default!;

    /// <summary>
    /// 上级部门 Id，可为空表示一级部门。
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 部门负责人，可为空，长度见 <see cref="DepartmentConsts.MaxLeaderLength"/>。
    /// </summary>
    [MaxLength(DepartmentConsts.MaxLeaderLength)]
    public string? Leader { get; set; }

    /// <summary>
    /// 联系电话，可为空；需满足手机号/电话格式。
    /// </summary>
    [Phone]
    [MaxLength(DepartmentConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 排序号，值越小越靠前，默认 0。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用，默认 true。
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 备注，可为空，长度见 <see cref="DepartmentConsts.MaxRemarkLength"/>。
    /// </summary>
    [MaxLength(DepartmentConsts.MaxRemarkLength)]
    public string? Remark { get; set; }
}
