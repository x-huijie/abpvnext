using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.OperationLogs;
using Xhj.Project.Permissions;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门应用服务：负责编排用例、权限校验与对象映射。
/// </summary>
/// <remarks>
/// 业务规则全部下沉到 <see cref="Department"/> 与 <see cref="DepartmentManager"/>，
/// 本类只做"取实体 → 调领域服务 → 持久化 → 映射返回"。
/// </remarks>
[Authorize(ProjectPermissions.Departments.Default)]
public class DepartmentAppService :
    CrudAppService<Department, DepartmentDto, Guid, GetDepartmentListInput, CreateUpdateDepartmentDto>,
    IDepartmentAppService
{
    private readonly DepartmentManager _departmentManager;
    private readonly IRepository<DepartmentMember, Guid> _departmentMemberRepository;
    private readonly IOperationLogWriter _operationLogWriter;

    /// <summary>
    /// 构造部门应用服务。
    /// </summary>
    /// <param name="repository">部门仓储。</param>
    /// <param name="departmentManager">部门领域服务。</param>
    /// <param name="departmentMemberRepository">部门成员仓储。</param>
    /// <param name="operationLogWriter">操作日志写入器，用于关键操作埋点。</param>
    public DepartmentAppService(
        IRepository<Department, Guid> repository,
        DepartmentManager departmentManager,
        IRepository<DepartmentMember, Guid> departmentMemberRepository,
        IOperationLogWriter operationLogWriter)
        : base(repository)
    {
        _departmentManager = departmentManager;
        _departmentMemberRepository = departmentMemberRepository;
        _operationLogWriter = operationLogWriter;
    }

    /// <summary>
    /// 新增部门。
    /// </summary>
    /// <param name="input">部门信息。</param>
    /// <returns>新建的部门。</returns>
    /// <exception cref="BusinessException">编码重复或上级不存在时抛出。</exception>
    [Authorize(ProjectPermissions.Departments.Create)]
    public override async Task<DepartmentDto> CreateAsync(CreateUpdateDepartmentDto input)
    {
        var department = await _departmentManager.CreateAsync(
            input.Name,
            input.Code,
            input.ParentId,
            input.Leader,
            input.PhoneNumber,
            input.Sort,
            input.Remark,
            input.IsActive);

        await Repository.InsertAsync(department, autoSave: true);

        // 埋点示例：新增部门属于需要追溯的重要业务操作
        await _operationLogWriter.WriteAsync(new WriteOperationLogInput
        {
            Module = nameof(Departments),
            Operation = "新增部门",
            OperationType = OperationType.Create,
            Description = $"新增部门：{department.Name}（编码 {department.Code}）",
            EntityType = typeof(Department).FullName,
            EntityId = department.Id.ToString()
        });

        return ObjectMapper.Map<Department, DepartmentDto>(department);
    }

    /// <summary>
    /// 编辑部门基本信息。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <param name="input">新的部门信息。</param>
    /// <returns>更新后的部门。</returns>
    [Authorize(ProjectPermissions.Departments.Update)]
    public override async Task<DepartmentDto> UpdateAsync(Guid id, CreateUpdateDepartmentDto input)
    {
        var department = await Repository.GetAsync(id);

        await _departmentManager.ChangeCodeAsync(department, input.Code);

        department
            .SetName(input.Name)
            .SetLeader(input.Leader)
            .SetPhoneNumber(input.PhoneNumber)
            .SetRemark(input.Remark)
            .SetSort(input.Sort)
            .SetActive(input.IsActive);

        // 上级部门变更走独立的 Move 接口，避免编辑时被误改层级
        await Repository.UpdateAsync(department, autoSave: true);

        return ObjectMapper.Map<Department, DepartmentDto>(department);
    }

    /// <summary>
    /// 删除部门。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <exception cref="BusinessException">存在子部门时抛出。</exception>
    [Authorize(ProjectPermissions.Departments.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        var department = await Repository.GetAsync(id);

        await _departmentManager.EnsureDeletableAsync(department);

        await Repository.DeleteAsync(department, autoSave: true);

        // 埋点示例：删除数据必须留痕
        await _operationLogWriter.WriteAsync(new WriteOperationLogInput
        {
            Module = nameof(Departments),
            Operation = "删除部门",
            OperationType = OperationType.Delete,
            Description = $"删除部门：{department.Name}（编码 {department.Code}）",
            EntityType = typeof(Department).FullName,
            EntityId = department.Id.ToString()
        });
    }

    /// <summary>
    /// 查询部门树，一次性返回全部层级。
    /// </summary>
    /// <param name="input">过滤条件；分页参数会被忽略。</param>
    /// <returns>根级部门及其递归子节点。</returns>
    public virtual async Task<ListResultDto<DepartmentTreeDto>> GetTreeAsync(GetDepartmentListInput input)
    {
        // 树结构一次性加载，不分页
        input.MaxResultCount = int.MaxValue;

        var query = await CreateFilteredQueryAsync(input);
        query = ApplySorting(query, input);

        var departments = await AsyncExecuter.ToListAsync(query);
        var nodes = departments
            .Select(x => ObjectMapper.Map<Department, DepartmentTreeDto>(x))
            .ToList();

        return new ListResultDto<DepartmentTreeDto>(BuildTree(nodes));
    }

    /// <summary>
    /// 调整部门层级。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <param name="input">新的上级部门。</param>
    /// <returns>调整后的部门。</returns>
    /// <exception cref="BusinessException">上级不存在或移动到自身/子孙时抛出。</exception>
    [Authorize(ProjectPermissions.Departments.Move)]
    public virtual async Task<DepartmentDto> MoveAsync(Guid id, MoveDepartmentInput input)
    {
        var department = await Repository.GetAsync(id);

        await _departmentManager.MoveAsync(department, input.NewParentId);

        await Repository.UpdateAsync(department, autoSave: true);

        return ObjectMapper.Map<Department, DepartmentDto>(department);
    }

    /// <summary>
    /// 查询部门成员。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <returns>成员列表。</returns>
    public virtual async Task<ListResultDto<DepartmentMemberDto>> GetMembersAsync(Guid id)
    {
        var query = await _departmentMemberRepository.GetQueryableAsync();

        var members = await AsyncExecuter.ToListAsync(
            query.Where(x => x.DepartmentId == id).OrderByDescending(x => x.IsPrimary));

        return new ListResultDto<DepartmentMemberDto>(
            members.Select(x => ObjectMapper.Map<DepartmentMember, DepartmentMemberDto>(x)).ToList());
    }

    /// <summary>
    /// 往部门中添加成员。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <param name="input">用户 Id 与是否主部门。</param>
    /// <returns>新建的成员关系。</returns>
    /// <exception cref="BusinessException">用户已在部门中时抛出。</exception>
    [Authorize(ProjectPermissions.Departments.ManageMembers)]
    public virtual async Task<DepartmentMemberDto> AddMemberAsync(Guid id, AddDepartmentMemberInput input)
    {
        // 先确认部门存在，避免产生指向空部门的关系
        await Repository.GetAsync(id);

        var member = await _departmentManager.CreateMemberAsync(id, input.UserId, input.IsPrimary);

        await _departmentMemberRepository.InsertAsync(member, autoSave: true);

        return ObjectMapper.Map<DepartmentMember, DepartmentMemberDto>(member);
    }

    /// <summary>
    /// 从部门中移除成员。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <param name="userId">用户 Id。</param>
    [Authorize(ProjectPermissions.Departments.ManageMembers)]
    public virtual async Task RemoveMemberAsync(Guid id, Guid userId)
    {
        var member = await _departmentMemberRepository.FirstOrDefaultAsync(x =>
            x.DepartmentId == id && x.UserId == userId);

        if (member == null)
        {
            return;
        }

        await _departmentMemberRepository.DeleteAsync(member, autoSave: true);
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    protected override async Task<IQueryable<Department>> CreateFilteredQueryAsync(GetDepartmentListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!));
        }

        if (input.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == input.ParentId.Value);
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        return query;
    }

    /// <summary>
    /// 由扁平列表组装树：ParentId 不在集合内的节点视为根节点。
    /// </summary>
    /// <param name="nodes">全部部门的扁平列表。</param>
    /// <returns>已按 Sort、Name 排序的根节点集合。</returns>
    /// <remarks>
    /// 用 ToLookup 一次分桶，避免逐节点全量扫描；
    /// 父节点被过滤条件排除时，其子节点会被提升为根节点。
    /// </remarks>
    private static List<DepartmentTreeDto> BuildTree(List<DepartmentTreeDto> nodes)
    {
        var childrenLookup = nodes.ToLookup(x => x.ParentId);

        foreach (var node in nodes)
        {
            node.Children = childrenLookup[node.Id]
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name)
                .ToList();
        }

        return nodes
            .Where(x => !x.ParentId.HasValue || !nodes.Any(p => p.Id == x.ParentId))
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .ToList();
    }
}
