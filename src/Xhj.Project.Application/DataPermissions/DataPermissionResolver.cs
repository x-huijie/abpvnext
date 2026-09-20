using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;
using Volo.Abp.Users;
using Xhj.Project.Departments;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限解析器实现：读取用户角色命中的规则，并把部门类范围展开为具体部门 Id。
/// </summary>
/// <remarks>
/// 多角色命中多条规则时按"<b>取最宽松</b>"合并：任一规则为 All 则放行；
/// 否则把各部门范围取并集，Self 与其它范围并存时以部门并集优先（能看部门数据必然能看自己的数据）。
/// </remarks>
public class DataPermissionResolver : IDataPermissionResolver, ITransientDependency
{
    private readonly IRepository<DataPermissionRule, Guid> _ruleRepository;
    private readonly IRepository<DepartmentMember, Guid> _departmentMemberRepository;
    private readonly IRepository<Department, Guid> _departmentRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 构造数据权限解析器。
    /// </summary>
    /// <param name="ruleRepository">数据权限规则仓储。</param>
    /// <param name="departmentMemberRepository">部门成员仓储，用于取用户所属部门。</param>
    /// <param name="departmentRepository">部门仓储，用于展开下级部门。</param>
    /// <param name="asyncExecuter">异步查询执行器。</param>
    /// <param name="currentUser">当前用户。</param>
    public DataPermissionResolver(
        IRepository<DataPermissionRule, Guid> ruleRepository,
        IRepository<DepartmentMember, Guid> departmentMemberRepository,
        IRepository<Department, Guid> departmentRepository,
        IAsyncQueryableExecuter asyncExecuter,
        ICurrentUser currentUser)
    {
        _ruleRepository = ruleRepository;
        _departmentMemberRepository = departmentMemberRepository;
        _departmentRepository = departmentRepository;
        _asyncExecuter = asyncExecuter;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 解析当前用户在指定资源上的数据权限。
    /// </summary>
    /// <param name="resourceKey">资源标识。</param>
    /// <returns>数据权限过滤结果。</returns>
    public virtual async Task<DataPermissionFilter> ResolveAsync(string resourceKey)
    {
        var roleNames = _currentUser.Roles;

        // 未登录或没有任何角色时不做过滤，交由接口自身的 [Authorize] 控制
        if (!_currentUser.IsAuthenticated || roleNames.Length == 0)
        {
            return DataPermissionFilter.All();
        }

        var rules = await _ruleRepository.GetListAsync(x =>
            x.IsEnabled && x.ResourceKey == resourceKey && roleNames.Contains(x.RoleName));

        if (rules.Count == 0)
        {
            // 未配置规则时默认放行，业务如需收紧可在此改为返回 Self
            return DataPermissionFilter.All();
        }

        if (rules.Any(x => x.Scope == DataPermissionScope.All))
        {
            return DataPermissionFilter.All(hasNoRule: false);
        }

        var departmentIds = new HashSet<Guid>();
        var selfOnly = false;

        foreach (var rule in rules)
        {
            switch (rule.Scope)
            {
                case DataPermissionScope.Self:
                    selfOnly = true;
                    break;

                case DataPermissionScope.Custom:
                    departmentIds.UnionWith(rule.GetDepartmentIds());
                    break;

                case DataPermissionScope.Department:
                    departmentIds.UnionWith(await GetUserDepartmentIdsAsync());
                    break;

                case DataPermissionScope.DepartmentAndChildren:
                    foreach (var departmentId in await GetUserDepartmentIdsAsync())
                    {
                        departmentIds.Add(departmentId);
                        departmentIds.UnionWith(await GetDescendantIdsAsync(departmentId));
                    }

                    break;
            }
        }

        if (departmentIds.Count == 0)
        {
            return new DataPermissionFilter
            {
                Scope = DataPermissionScope.Self,
                CurrentUserId = _currentUser.Id,
                HasNoRule = !selfOnly
            };
        }

        return new DataPermissionFilter
        {
            Scope = DataPermissionScope.Custom,
            DepartmentIds = departmentIds.ToList(),
            CurrentUserId = _currentUser.Id
        };
    }

    /// <summary>
    /// 取当前用户所属部门 Id（含主部门与兼任部门）。
    /// </summary>
    /// <returns>部门 Id 集合。</returns>
    private async Task<List<Guid>> GetUserDepartmentIdsAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return new List<Guid>();
        }

        var query = await _departmentMemberRepository.GetQueryableAsync();

        return await _asyncExecuter.ToListAsync(
            query.Where(x => x.UserId == _currentUser.Id.Value).Select(x => x.DepartmentId));
    }

    /// <summary>
    /// 取指定部门的所有下级部门 Id（递归）。
    /// </summary>
    /// <param name="departmentId">部门 Id。</param>
    /// <returns>下级部门 Id 集合（不含自身）。</returns>
    /// <remarks>
    /// 部门数量有限，一次性载入内存后用 BFS 展开，避免递归查询数据库。
    /// </remarks>
    private async Task<List<Guid>> GetDescendantIdsAsync(Guid departmentId)
    {
        var departments = await _departmentRepository.GetListAsync();
        var result = new List<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(departmentId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = departments.Where(x => x.ParentId == current).ToList();

            foreach (var child in children)
            {
                if (result.Contains(child.Id))
                {
                    continue;
                }

                result.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }

        return result;
    }
}
