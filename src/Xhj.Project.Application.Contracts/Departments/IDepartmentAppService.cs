using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Xhj.Project.Excel;

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

    /// <summary>
    /// 查询部门成员。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <returns>该部门的成员列表。</returns>
    Task<ListResultDto<DepartmentMemberDto>> GetMembersAsync(Guid id);

    /// <summary>
    /// 往部门中添加成员（数据权限的"用户-部门"归属来源）。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <param name="input">用户 Id 与是否主部门。</param>
    /// <returns>新建的成员关系。</returns>
    Task<DepartmentMemberDto> AddMemberAsync(Guid id, AddDepartmentMemberInput input);

    /// <summary>
    /// 从部门中移除成员。
    /// </summary>
    /// <param name="id">部门 Id。</param>
    /// <param name="userId">用户 Id。</param>
    Task RemoveMemberAsync(Guid id, Guid userId);

    /// <summary>
    /// 按当前筛选条件导出部门为 Excel。
    /// </summary>
    /// <param name="input">与列表一致的筛选与排序条件，分页参数会被忽略。</param>
    /// <returns>xlsx 文件。</returns>
    Task<IRemoteStreamContent> ExportAsync(GetDepartmentListInput input);

    /// <summary>
    /// 从 Excel 批量导入部门。
    /// </summary>
    /// <param name="file">xlsx 文件，表头需与 <see cref="DepartmentImportDto"/> 一致。</param>
    /// <returns>导入结果（成功/失败明细），失败行不影响其他行。</returns>
    Task<ImportResultDto> ImportAsync(IRemoteStreamContent file);
}
