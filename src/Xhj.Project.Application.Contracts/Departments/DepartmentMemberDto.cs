using System;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门成员输出 DTO。
/// </summary>
public class DepartmentMemberDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 部门 Id。
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// 用户 Id。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 是否为主部门；数据权限计算时优先使用主部门。
    /// </summary>
    public bool IsPrimary { get; set; }
}
