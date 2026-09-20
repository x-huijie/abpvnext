using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xhj.Project.DataPermissions;

/// <summary>
/// 把 <see cref="DataPermissionFilter"/> 应用为 EF 查询条件的扩展方法。
/// </summary>
/// <remarks>
/// 业务查询只需两行代码即可接入数据权限：
/// <code>
/// var filter = await _dataPermissionResolver.ResolveAsync("Products");
/// query = query.ApplyCreatorDataPermission(filter, x =&gt; x.CreatorId)
///              .ApplyDepartmentDataPermission(filter, x =&gt; x.DepartmentId);
/// </code>
/// </remarks>
public static class DataPermissionQueryExtensions
{
    /// <summary>
    /// 按"仅本人"范围过滤创建人。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <param name="query">原始查询。</param>
    /// <param name="filter">数据权限过滤结果。</param>
    /// <param name="creatorIdSelector">实体上创建人 Id 的选择器。</param>
    /// <returns>应用过滤后的查询。</returns>
    public static IQueryable<TEntity> ApplyCreatorDataPermission<TEntity>(
        this IQueryable<TEntity> query,
        DataPermissionFilter filter,
        Expression<Func<TEntity, Guid?>> creatorIdSelector)
        where TEntity : class
    {
        if (filter == null || filter.IsAll)
        {
            return query;
        }

        if (filter.Scope != DataPermissionScope.Self)
        {
            return query;
        }

        // 未登录用户没有任何自己的数据，直接返回空结果
        if (!filter.CurrentUserId.HasValue)
        {
            return query.Where(Expression.Lambda<Func<TEntity, bool>>(
                Expression.Constant(false),
                creatorIdSelector.Parameters));
        }

        var body = Expression.Equal(
            creatorIdSelector.Body,
            Expression.Constant(filter.CurrentUserId, typeof(Guid?)));

        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, creatorIdSelector.Parameters);

        return query.Where(predicate);
    }

    /// <summary>
    /// 按部门范围过滤（本部门、本部门及下级、自定义部门统一按部门 Id 集合过滤）。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <param name="query">原始查询。</param>
    /// <param name="filter">数据权限过滤结果。</param>
    /// <param name="departmentIdSelector">实体上部门 Id 的选择器。</param>
    /// <returns>应用过滤后的查询。</returns>
    public static IQueryable<TEntity> ApplyDepartmentDataPermission<TEntity>(
        this IQueryable<TEntity> query,
        DataPermissionFilter filter,
        Expression<Func<TEntity, Guid>> departmentIdSelector)
        where TEntity : class
    {
        if (filter == null || filter.IsAll)
        {
            return query;
        }

        if (filter.DepartmentIds == null || filter.DepartmentIds.Count == 0)
        {
            return query;
        }

        // 手工拼 Contains 表达式，使任意实体都能复用同一套过滤逻辑
        var containsMethod = typeof(List<Guid>).GetMethod(nameof(List<Guid>.Contains))!;
        var body = Expression.Call(
            Expression.Constant(filter.DepartmentIds),
            containsMethod,
            departmentIdSelector.Body);

        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, departmentIdSelector.Parameters);

        return query.Where(predicate);
    }
}
