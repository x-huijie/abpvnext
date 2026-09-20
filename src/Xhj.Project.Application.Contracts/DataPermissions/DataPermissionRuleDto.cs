using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限规则输出 DTO。
/// </summary>
public class DataPermissionRuleDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 所属租户 Id；host 数据为 null。
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 角色 Id。
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 角色名（冗余展示）。
    /// </summary>
    public string RoleName { get; set; } = default!;

    /// <summary>
    /// 资源标识，如 "Products"。
    /// </summary>
    public string ResourceKey { get; set; } = default!;

    /// <summary>
    /// 数据范围。
    /// </summary>
    public DataPermissionScope Scope { get; set; }

    /// <summary>
    /// 自定义范围的部门 Id 集合；非 Custom 范围为空集合。
    /// </summary>
    public List<Guid> DepartmentIds { get; set; } = new();

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; }
}
