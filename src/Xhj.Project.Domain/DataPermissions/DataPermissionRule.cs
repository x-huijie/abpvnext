using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限规则聚合根：定义"某角色在某资源上能看到什么范围的数据"。
/// </summary>
/// <remarks>
/// 不变式（由 <see cref="DataPermissionManager"/> 保证）：
/// 1. 同一角色在同一资源上只能配置一条规则；
/// 2. 范围为 <see cref="DataPermissionScope.Custom"/> 时必须至少指定一个部门；
/// 3. 部门 Id 集合以逗号分隔存储，避免引入额外的关联表。
/// </remarks>
public class DataPermissionRule : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 角色 Id（IdentityRole）。
    /// </summary>
    public virtual Guid RoleId { get; protected set; }

    /// <summary>
    /// 角色名（冗余存储，便于列表直接展示，避免联表）。
    /// </summary>
    public virtual string RoleName { get; protected set; } = default!;

    /// <summary>
    /// 资源标识，如 "Products""Orders"，与业务模块约定一致。
    /// </summary>
    public virtual string ResourceKey { get; protected set; } = default!;

    /// <summary>
    /// 数据范围。
    /// </summary>
    public virtual DataPermissionScope Scope { get; protected set; }

    /// <summary>
    /// 自定义部门 Id 集合（逗号分隔），仅 <see cref="DataPermissionScope.Custom"/> 使用。
    /// </summary>
    public virtual string? DepartmentIds { get; protected set; }

    /// <summary>
    /// 是否启用；停用后该规则不参与计算。
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected DataPermissionRule()
    {
        RoleName = string.Empty;
        ResourceKey = string.Empty;
    }

    /// <summary>
    /// 创建数据权限规则。
    /// </summary>
    /// <param name="id">聚合根 Id。</param>
    /// <param name="roleId">角色 Id。</param>
    /// <param name="roleName">角色名。</param>
    /// <param name="resourceKey">资源标识。</param>
    /// <param name="scope">数据范围。</param>
    /// <param name="departmentIds">自定义部门 Id 集合，可为空。</param>
    /// <param name="isEnabled">是否启用。</param>
    public DataPermissionRule(
        Guid id,
        Guid roleId,
        string roleName,
        string resourceKey,
        DataPermissionScope scope,
        string? departmentIds = null,
        bool isEnabled = true)
        : base(id)
    {
        RoleId = roleId;
        SetRoleName(roleName);
        SetResourceKey(resourceKey);
        SetScope(scope);
        SetDepartmentIds(departmentIds);
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// 修改角色名。
    /// </summary>
    /// <param name="roleName">角色名。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DataPermissionRule SetRoleName(string roleName)
    {
        Check.NotNullOrWhiteSpace(roleName, nameof(roleName), DataPermissionConsts.MaxRoleNameLength);

        RoleName = roleName;
        return this;
    }

    /// <summary>
    /// 修改资源标识。
    /// </summary>
    /// <param name="resourceKey">资源标识。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DataPermissionRule SetResourceKey(string resourceKey)
    {
        Check.NotNullOrWhiteSpace(resourceKey, nameof(resourceKey), DataPermissionConsts.MaxResourceKeyLength);

        ResourceKey = resourceKey;
        return this;
    }

    /// <summary>
    /// 修改数据范围。
    /// </summary>
    /// <param name="scope">新的数据范围。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DataPermissionRule SetScope(DataPermissionScope scope)
    {
        Scope = scope;
        return this;
    }

    /// <summary>
    /// 修改自定义部门 Id 集合（以逗号分隔），传入 null 或空表示清空。
    /// </summary>
    /// <param name="departmentIds">部门 Id 集合字符串。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DataPermissionRule SetDepartmentIds(string? departmentIds)
    {
        DepartmentIds = departmentIds.IsNullOrWhiteSpace() ? null : departmentIds;
        return this;
    }

    /// <summary>
    /// 启用/停用该规则。
    /// </summary>
    /// <param name="isEnabled">true 为启用。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual DataPermissionRule SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// 解析出自定义部门 Id 集合。
    /// </summary>
    /// <returns>部门 Id 集合；未配置时返回空集合。</returns>
    public virtual List<Guid> GetDepartmentIds()
    {
        if (DepartmentIds.IsNullOrWhiteSpace())
        {
            return new List<Guid>();
        }

        return DepartmentIds!
            .Split(DataPermissionConsts.DepartmentIdSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Guid.TryParse(x.Trim(), out var id) ? id : (Guid?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
    }
}
