using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限规则应用服务：维护"角色 × 资源 × 范围"的授权规则。
/// </summary>
[Authorize(ProjectPermissions.DataPermissions.Default)]
public class DataPermissionRuleAppService :
    CrudAppService<DataPermissionRule, DataPermissionRuleDto, Guid, GetDataPermissionRuleListInput,
        CreateUpdateDataPermissionRuleDto>,
    IDataPermissionRuleAppService
{
    private readonly DataPermissionManager _dataPermissionManager;

    /// <summary>
    /// 构造数据权限规则应用服务。
    /// </summary>
    /// <param name="repository">规则仓储。</param>
    /// <param name="dataPermissionManager">数据权限领域服务。</param>
    public DataPermissionRuleAppService(
        IRepository<DataPermissionRule, Guid> repository,
        DataPermissionManager dataPermissionManager)
        : base(repository)
    {
        _dataPermissionManager = dataPermissionManager;
    }

    /// <summary>
    /// 新增规则。
    /// </summary>
    /// <param name="input">规则信息。</param>
    /// <returns>新建的规则。</returns>
    /// <exception cref="BusinessException">规则重复或自定义范围未指定部门时抛出。</exception>
    [Authorize(ProjectPermissions.DataPermissions.Create)]
    public override async Task<DataPermissionRuleDto> CreateAsync(CreateUpdateDataPermissionRuleDto input)
    {
        var rule = await _dataPermissionManager.CreateAsync(
            input.RoleId,
            input.RoleName,
            input.ResourceKey,
            input.Scope,
            BuildDepartmentIds(input.DepartmentIds),
            input.IsEnabled);

        await Repository.InsertAsync(rule, autoSave: true);

        return ObjectMapper.Map<DataPermissionRule, DataPermissionRuleDto>(rule);
    }

    /// <summary>
    /// 编辑规则。
    /// </summary>
    /// <param name="id">规则 Id。</param>
    /// <param name="input">新的规则信息。</param>
    /// <returns>更新后的规则。</returns>
    [Authorize(ProjectPermissions.DataPermissions.Update)]
    public override async Task<DataPermissionRuleDto> UpdateAsync(Guid id, CreateUpdateDataPermissionRuleDto input)
    {
        var rule = await Repository.GetAsync(id);

        _dataPermissionManager.ChangeScope(rule, input.Scope, BuildDepartmentIds(input.DepartmentIds));

        rule.SetRoleName(input.RoleName).SetResourceKey(input.ResourceKey).SetEnabled(input.IsEnabled);

        await Repository.UpdateAsync(rule, autoSave: true);

        return ObjectMapper.Map<DataPermissionRule, DataPermissionRuleDto>(rule);
    }

    /// <summary>
    /// 删除规则。
    /// </summary>
    /// <param name="id">规则 Id。</param>
    [Authorize(ProjectPermissions.DataPermissions.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        await Repository.DeleteAsync(id, autoSave: true);
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    protected override async Task<IQueryable<DataPermissionRule>> CreateFilteredQueryAsync(
        GetDataPermissionRuleListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (input.RoleId.HasValue)
        {
            query = query.Where(x => x.RoleId == input.RoleId.Value);
        }

        if (!input.ResourceKey.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.ResourceKey == input.ResourceKey);
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        return query;
    }

    /// <summary>
    /// 映射实体到输出 DTO，并补上由字符串解析出的部门 Id 集合。
    /// </summary>
    /// <param name="entity">数据权限规则实体。</param>
    /// <returns>规则 DTO。</returns>
    protected override Task<DataPermissionRuleDto> MapToGetOutputDtoAsync(DataPermissionRule entity)
    {
        var dto = ObjectMapper.Map<DataPermissionRule, DataPermissionRuleDto>(entity);

        // DepartmentIds 在实体中是逗号分隔字符串，DTO 中是集合，映射后单独补齐
        dto.DepartmentIds = entity.GetDepartmentIds();

        return Task.FromResult(dto);
    }

    /// <summary>
    /// 把部门 Id 集合序列化为逗号分隔字符串。
    /// </summary>
    /// <param name="departmentIds">部门 Id 集合，可为空。</param>
    /// <returns>逗号分隔字符串；空集合时返回 null。</returns>
    private static string? BuildDepartmentIds(System.Collections.Generic.List<Guid>? departmentIds)
    {
        return departmentIds == null || departmentIds.Count == 0
            ? null
            : string.Join(DataPermissionConsts.DepartmentIdSeparator, departmentIds);
    }
}
