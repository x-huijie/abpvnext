using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Excel;
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
    private readonly IExcelExporter _excelExporter;
    private readonly IExcelImporter _excelImporter;

    /// <summary>
    /// 构造部门应用服务。
    /// </summary>
    /// <param name="repository">部门仓储。</param>
    /// <param name="departmentManager">部门领域服务。</param>
    /// <param name="departmentMemberRepository">部门成员仓储。</param>
    /// <param name="operationLogWriter">操作日志写入器，用于关键操作埋点。</param>
    /// <param name="excelExporter">Excel 导出器。</param>
    /// <param name="excelImporter">Excel 导入器。</param>
    public DepartmentAppService(
        IRepository<Department, Guid> repository,
        DepartmentManager departmentManager,
        IRepository<DepartmentMember, Guid> departmentMemberRepository,
        IOperationLogWriter operationLogWriter,
        IExcelExporter excelExporter,
        IExcelImporter excelImporter)
        : base(repository)
    {
        _departmentManager = departmentManager;
        _departmentMemberRepository = departmentMemberRepository;
        _operationLogWriter = operationLogWriter;
        _excelExporter = excelExporter;
        _excelImporter = excelImporter;
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
    /// 导出列的中英文表头映射。
    /// </summary>
    private static readonly Dictionary<string, string> DepartmentExportHeaders = new()
    {
        [nameof(DepartmentDto.Name)] = "部门名称",
        [nameof(DepartmentDto.Code)] = "部门编码",
        [nameof(DepartmentDto.Leader)] = "负责人",
        [nameof(DepartmentDto.PhoneNumber)] = "联系电话",
        [nameof(DepartmentDto.Sort)] = "排序",
        [nameof(DepartmentDto.IsActive)] = "是否启用",
        [nameof(DepartmentDto.Remark)] = "备注"
    };

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
    /// 按当前筛选条件导出部门为 Excel。
    /// </summary>
    /// <param name="input">与列表一致的筛选与排序条件，分页参数会被忽略。</param>
    /// <returns>xlsx 文件。</returns>
    public virtual async Task<IRemoteStreamContent> ExportAsync(GetDepartmentListInput input)
    {
        // 导出不分页，按列表同样的筛选与排序条件取全量
        input.MaxResultCount = int.MaxValue;

        var query = await CreateFilteredQueryAsync(input);
        query = ApplySorting(query, input);

        var departments = await AsyncExecuter.ToListAsync(query);
        var dtos = departments.Select(x => ObjectMapper.Map<Department, DepartmentDto>(x)).ToList();

        var bytes = await _excelExporter.ExportAsync(dtos, "部门", DepartmentExportHeaders);

        return new RemoteStreamContent(
            new MemoryStream(bytes),
            $"部门_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            ExcelConsts.ExcelContentType);
    }

    /// <summary>
    /// 从 Excel 批量导入部门。
    /// </summary>
    /// <param name="file">xlsx 文件。</param>
    /// <returns>导入结果，单行失败不影响其他行。</returns>
    [Authorize(ProjectPermissions.Departments.Create)]
    public virtual async Task<ImportResultDto> ImportAsync(IRemoteStreamContent file)
    {
        var result = new ImportResultDto();

        var rows = await _excelImporter.ImportAsync<DepartmentImportDto>(file.GetStream());
        result.TotalCount = rows.Count;

        var currentDepartments = await AsyncExecuter.ToListAsync(await Repository.GetQueryableAsync());
        var codeToId = currentDepartments.ToDictionary(x => x.Code, x => x.Id);

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowNumber = index + 1;

            try
            {
                await ImportSingleRowAsync(row, codeToId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                // 单行失败只记录原因，继续处理后续行
                result.FailedCount++;
                result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = rowNumber,
                    RowContent = $"{row.Code}/{row.Name}",
                    Message = ex.Message
                });
            }
        }

        await _operationLogWriter.WriteAsync(new WriteOperationLogInput
        {
            Module = nameof(Departments),
            Operation = "批量导入部门",
            OperationType = OperationType.Import,
            Description = $"共 {result.TotalCount} 行，成功 {result.SuccessCount} 行，失败 {result.FailedCount} 行",
            IsSuccess = result.FailedCount == 0
        });

        return result;
    }

    /// <summary>
    /// 导入单行数据：把上级编码转换为 Id 后交由领域服务创建。
    /// </summary>
    /// <param name="row">Excel 行数据。</param>
    /// <param name="codeToId">已有部门的"编码 → Id"映射，同时用于解析上级部门。</param>
    /// <exception cref="BusinessException">必填缺失、编码重复或上级不存在时抛出。</exception>
    private async Task ImportSingleRowAsync(DepartmentImportDto row, Dictionary<string, Guid> codeToId)
    {
        if (row.Name.IsNullOrWhiteSpace() || row.Code.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ProjectDomainErrorCodes.DepartmentCodeAlreadyExists)
                .WithData("Message", "部门名称与编码不能为空。");
        }

        Guid? parentId = null;

        if (!row.ParentCode.IsNullOrWhiteSpace())
        {
            parentId = codeToId.TryGetValue(row.ParentCode!, out var id)
                ? id
                : throw new BusinessException(ProjectDomainErrorCodes.ParentDepartmentNotFound)
                    .WithData("Code", row.ParentCode);
        }

        var department = await _departmentManager.CreateAsync(
            row.Name!,
            row.Code!,
            parentId,
            row.Leader,
            row.PhoneNumber,
            row.Sort,
            row.Remark,
            row.IsActive);

        await Repository.InsertAsync(department, autoSave: true);

        // 后续行可能以本行为上级，因此即时更新映射
        codeToId[department.Code] = department.Id;
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
