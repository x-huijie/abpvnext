using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门领域服务：维护部门树的全部不变式。
/// </summary>
/// <remarks>
/// 涉及"跨记录校验"（编码唯一、上级存在、子树判断）的规则必须放在这里，
/// 单个实体自身能校验的规则（必填、长度）放在 <see cref="Department"/> 内部。
/// </remarks>
public class DepartmentManager : DomainService
{
    private readonly IRepository<Department, Guid> _departmentRepository;

    /// <summary>
    /// 构造部门领域服务。
    /// </summary>
    /// <param name="departmentRepository">部门仓储。</param>
    public DepartmentManager(IRepository<Department, Guid> departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    /// <summary>
    /// 创建部门：校验编码唯一与上级存在后返回新实体（未持久化）。
    /// </summary>
    /// <param name="name">部门名称。</param>
    /// <param name="code">部门编码。</param>
    /// <param name="parentId">上级部门 Id，可为空。</param>
    /// <param name="leader">负责人，可为空。</param>
    /// <param name="phoneNumber">联系电话，可为空。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="remark">备注，可为空。</param>
    /// <param name="isActive">是否启用。</param>
    /// <returns>新建的部门聚合根。</returns>
    /// <exception cref="BusinessException">编码重复或上级不存在时抛出。</exception>
    public virtual async Task<Department> CreateAsync(
        string name,
        string code,
        Guid? parentId,
        string? leader,
        string? phoneNumber,
        int sort,
        string? remark,
        bool isActive)
    {
        await CheckCodeAsync(code);
        await EnsureParentExistsAsync(parentId);

        return new Department(
            GuidGenerator.Create(),
            name,
            code,
            parentId,
            leader,
            phoneNumber,
            sort,
            remark,
            isActive);
    }

    /// <summary>
    /// 变更部门编码；编码未变化则直接跳过校验。
    /// </summary>
    /// <param name="department">目标部门。</param>
    /// <param name="code">新的部门编码。</param>
    /// <exception cref="BusinessException">编码已被其他部门占用时抛出。</exception>
    public virtual async Task ChangeCodeAsync(Department department, string code)
    {
        if (string.Equals(department.Code, code, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await CheckCodeAsync(code, department.Id);

        department.SetCode(code);
    }

    /// <summary>
    /// 变更上级部门：不允许移动到自己或其子孙节点下。
    /// </summary>
    /// <param name="department">待移动的部门。</param>
    /// <param name="newParentId">新的上级 Id；为 null 表示挂到根级。</param>
    /// <exception cref="BusinessException">上级不存在，或移动到自身/子孙节点时抛出。</exception>
    public virtual async Task MoveAsync(Department department, Guid? newParentId)
    {
        if (!newParentId.HasValue)
        {
            department.SetParent(null);
            return;
        }

        if (newParentId.Value == department.Id)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CannotMoveDepartmentToChild);
        }

        await EnsureParentExistsAsync(newParentId);

        if (await IsDescendantAsync(newParentId.Value, department.Id))
        {
            throw new BusinessException(ProjectDomainErrorCodes.CannotMoveDepartmentToChild);
        }

        department.SetParent(newParentId);
    }

    /// <summary>
    /// 删除前校验：存在子部门时拒绝删除。
    /// </summary>
    /// <param name="department">待删除的部门。</param>
    /// <exception cref="BusinessException">存在子部门时抛出。</exception>
    public virtual async Task EnsureDeletableAsync(Department department)
    {
        var hasChildren = await _departmentRepository.AnyAsync(x => x.ParentId == department.Id);

        if (hasChildren)
        {
            throw new BusinessException(ProjectDomainErrorCodes.DepartmentHasChildren)
                .WithData("Name", department.Name);
        }
    }

    /// <summary>
    /// 校验部门编码在租户内是否可用。
    /// </summary>
    /// <param name="code">待校验的编码。</param>
    /// <param name="exceptId">排除的部门 Id（用于更新场景），可为空。</param>
    /// <exception cref="BusinessException">编码已被占用时抛出。</exception>
    private async Task CheckCodeAsync(string code, Guid? exceptId = null)
    {
        // 仓储已按当前租户过滤，因此只需比对编码本身
        var exists = exceptId.HasValue
            ? await _departmentRepository.AnyAsync(x => x.Code == code && x.Id != exceptId.Value)
            : await _departmentRepository.AnyAsync(x => x.Code == code);

        if (exists)
        {
            throw new BusinessException(ProjectDomainErrorCodes.DepartmentCodeAlreadyExists)
                .WithData("Code", code);
        }
    }

    /// <summary>
    /// 校验上级部门是否存在。
    /// </summary>
    /// <param name="parentId">上级部门 Id，可为空表示根级。</param>
    /// <exception cref="BusinessException">上级部门不存在时抛出。</exception>
    private async Task EnsureParentExistsAsync(Guid? parentId)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        var parent = await _departmentRepository.FindAsync(parentId.Value);

        if (parent == null)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ParentDepartmentNotFound);
        }
    }

    /// <summary>
    /// 判断 candidateId 是否位于 ancestorId 的子树中。
    /// </summary>
    /// <param name="candidateId">待判断的节点 Id。</param>
    /// <param name="ancestorId">可能的祖先节点 Id。</param>
    /// <returns>candidateId 在 ancestorId 子树中返回 true。</returns>
    /// <remarks>
    /// 部门数量有限（组织机构通常百级以内），一次性载入后在内存中沿 ParentId 上溯，
    /// 比递归查询数据库更简单可靠。
    /// </remarks>
    private async Task<bool> IsDescendantAsync(Guid candidateId, Guid ancestorId)
    {
        var departments = await _departmentRepository.GetListAsync();
        var current = departments.FirstOrDefault(x => x.Id == candidateId);

        while (current?.ParentId != null)
        {
            if (current.ParentId.Value == ancestorId)
            {
                return true;
            }

            current = departments.FirstOrDefault(x => x.Id == current.ParentId.Value);
        }

        return false;
    }
}
