using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门（组织机构）应用服务，提供 CRUD、树查询与层级调整。
/// </summary>
public interface IDepartmentAppService :
    ICrudAppService<DepartmentDto, Guid, GetDepartmentListInput, CreateUpdateDepartmentDto>
{
    /// <summary>
    /// 部门树（不分页），供下拉/树形控件一次性加载。
    /// </summary>
    /// <param name="input">过滤与排序条件，分页参数会被忽略。</param>
    /// <returns>根级部门集合，Children 递归嵌套。</returns>
    Task<ListResultDto<DepartmentTreeDto>> GetTreeAsync(GetDepartmentListInput input);

    /// <summary>
    /// 调整部门所属上级。
    /// </summary>
    /// <param name="id">待调整的部门 Id。</param>
    /// <param name="input">新的上级部门 Id。</param>
    /// <returns>调整后的部门。</returns>
    Task<DepartmentDto> MoveAsync(Guid id, MoveDepartmentInput input);
}
