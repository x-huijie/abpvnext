using System.Threading.Tasks;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 数据权限解析器：把"当前用户的角色 + 资源标识"解析为可执行的过滤条件。
/// </summary>
public interface IDataPermissionResolver
{
    /// <summary>
    /// 解析当前用户在指定资源上的数据权限。
    /// </summary>
    /// <param name="resourceKey">资源标识，与规则中配置的值一致，如 "Products"。</param>
    /// <returns>数据权限过滤结果。</returns>
    Task<DataPermissionFilter> ResolveAsync(string resourceKey);
}
