using System;
using System.ComponentModel.DataAnnotations;

namespace Xhj.Project.Departments;

/// <summary>
/// 往部门中添加成员的入参。
/// </summary>
public class AddDepartmentMemberInput
{
    /// <summary>
    /// 用户 Id，必填。
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 是否设为该用户的主部门，默认 false。
    /// </summary>
    public bool IsPrimary { get; set; }
}
