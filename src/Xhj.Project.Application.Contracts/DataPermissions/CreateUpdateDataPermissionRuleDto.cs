using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限规则新增/编辑入参。
/// </summary>
public class CreateUpdateDataPermissionRuleDto
{
    /// <summary>
    /// 角色 Id，必填。
    /// </summary>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 角色名，必填（冗余存储，便于列表展示）。
    /// </summary>
    [Required]
    [MaxLength(DataPermissionConsts.MaxRoleNameLength)]
    public string RoleName { get; set; } = default!;

    /// <summary>
    /// 资源标识，必填，建议与业务模块名一致。
    /// </summary>
    [Required]
    [MaxLength(DataPermissionConsts.MaxResourceKeyLength)]
    public string ResourceKey { get; set; } = default!;

    /// <summary>
    /// 数据范围，必填。
    /// </summary>
    [Required]
    public DataPermissionScope Scope { get; set; }

    /// <summary>
    /// 自定义部门 Id 集合；Scope 为 Custom 时必填且不能为空。
    /// </summary>
    public List<Guid> DepartmentIds { get; set; } = new();

    /// <summary>
    /// 是否启用，默认 true。
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
