using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门成员聚合根，描述"用户属于哪个部门"，是数据权限按部门过滤的数据基础。
/// </summary>
/// <remarks>
/// 一个用户可以属于多个部门；同一部门内同一用户只允许一条记录（由 <see cref="DepartmentManager"/> 保证）。
/// 不做导航属性，部门与用户均以 Id 引用，避免跨聚合耦合。
/// </remarks>
public class DepartmentMember : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 部门 Id。
    /// </summary>
    public virtual Guid DepartmentId { get; protected set; }

    /// <summary>
    /// 用户 Id（IdentityUser）。
    /// </summary>
    public virtual Guid UserId { get; protected set; }

    /// <summary>
    /// 是否为该用户的主部门；数据权限计算时优先使用主部门。
    /// </summary>
    public virtual bool IsPrimary { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected DepartmentMember()
    {
    }

    /// <summary>
    /// 创建部门成员关系。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="departmentId">部门 Id。</param>
    /// <param name="userId">用户 Id。</param>
    /// <param name="isPrimary">是否为主部门。</param>
    public DepartmentMember(Guid id, Guid departmentId, Guid userId, bool isPrimary = false)
        : base(id)
    {
        DepartmentId = departmentId;
        UserId = userId;
        IsPrimary = isPrimary;
    }

    /// <summary>
    /// 设置是否为主部门。
    /// </summary>
    /// <param name="isPrimary">true 表示主部门。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DepartmentMember SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        return this;
    }
}
