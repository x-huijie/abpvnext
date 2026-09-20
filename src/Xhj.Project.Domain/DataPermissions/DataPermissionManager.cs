using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限领域服务：保证规则唯一性，并把"角色 + 资源"解析为可执行的过滤条件。
/// </summary>
public class DataPermissionManager : DomainService
{
    private readonly IRepository<DataPermissionRule, Guid> _ruleRepository;

    /// <summary>
    /// 构造数据权限领域服务。
    /// </summary>
    /// <param name="ruleRepository">数据权限规则仓储。</param>
    public DataPermissionManager(IRepository<DataPermissionRule, Guid> ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    /// <summary>
    /// 创建规则：校验唯一性与自定义范围的部门必填（未持久化）。
    /// </summary>
    /// <param name="roleId">角色 Id。</param>
    /// <param name="roleName">角色名。</param>
    /// <param name="resourceKey">资源标识。</param>
    /// <param name="scope">数据范围。</param>
    /// <param name="departmentIds">自定义部门 Id 集合，可为空。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <returns>新建的规则。</returns>
    /// <exception cref="BusinessException">规则重复，或自定义范围未指定部门时抛出。</exception>
    public virtual async Task<DataPermissionRule> CreateAsync(
        Guid roleId,
        string roleName,
        string resourceKey,
        DataPermissionScope scope,
        string? departmentIds,
        bool isEnabled)
    {
        await CheckRuleAsync(roleId, resourceKey);

        var rule = new DataPermissionRule(
            GuidGenerator.Create(),
            roleId,
            roleName,
            resourceKey,
            scope,
            departmentIds,
            isEnabled);

        EnsureCustomScopeIsValid(rule);

        return rule;
    }

    /// <summary>
    /// 变更规则的范围与部门集合，并校验自定义范围的部门必填。
    /// </summary>
    /// <param name="rule">目标规则。</param>
    /// <param name="scope">新的数据范围。</param>
    /// <param name="departmentIds">新的部门集合，可为空。</param>
    public virtual void ChangeScope(DataPermissionRule rule, DataPermissionScope scope, string? departmentIds)
    {
        rule.SetScope(scope);
        rule.SetDepartmentIds(departmentIds);

        EnsureCustomScopeIsValid(rule);
    }

    /// <summary>
    /// 校验同一角色在同一资源上的规则唯一性。
    /// </summary>
    /// <param name="roleId">角色 Id。</param>
    /// <param name="resourceKey">资源标识。</param>
    /// <param name="exceptId">排除的规则 Id（更新场景），可为空。</param>
    /// <exception cref="BusinessException">已存在规则时抛出。</exception>
    private async Task CheckRuleAsync(Guid roleId, string resourceKey, Guid? exceptId = null)
    {
        var exists = exceptId.HasValue
            ? await _ruleRepository.AnyAsync(x =>
                x.RoleId == roleId && x.ResourceKey == resourceKey && x.Id != exceptId.Value)
            : await _ruleRepository.AnyAsync(x => x.RoleId == roleId && x.ResourceKey == resourceKey);

        if (exists)
        {
            throw new BusinessException(ProjectDomainErrorCodes.DataPermissionRuleAlreadyExists)
                .WithData("ResourceKey", resourceKey);
        }
    }

    /// <summary>
    /// 校验自定义范围必须指定至少一个部门。
    /// </summary>
    /// <param name="rule">待校验的规则。</param>
    /// <exception cref="BusinessException">范围为 Custom 但部门集合为空时抛出。</exception>
    private static void EnsureCustomScopeIsValid(DataPermissionRule rule)
    {
        if (rule.Scope != DataPermissionScope.Custom)
        {
            return;
        }

        // 自定义范围没有部门时等价于"看不到任何数据"，属于配置错误，直接拒绝
        if (rule.GetDepartmentIds().Count == 0)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CustomScopeDepartmentRequired);
        }
    }
}
