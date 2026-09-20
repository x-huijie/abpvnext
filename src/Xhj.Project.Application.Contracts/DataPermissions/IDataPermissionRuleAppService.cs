using System;
using Volo.Abp.Application.Services;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限规则应用服务：维护"角色 × 资源 × 范围"的授权规则。
/// </summary>
/// <remarks>
/// 规则只描述范围，真正的数据过滤由 <c>IDataPermissionResolver</c> 在业务查询时应用。
/// </remarks>
public interface IDataPermissionRuleAppService :
    ICrudAppService<DataPermissionRuleDto, Guid, GetDataPermissionRuleListInput, CreateUpdateDataPermissionRuleDto>
{
}
