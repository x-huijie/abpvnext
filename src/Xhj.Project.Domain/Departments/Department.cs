using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Xhj.Project.Departments;

/// <summary>
/// 部门（组织机构）聚合根，通过 <see cref="ParentId"/> 自引用形成树形结构。
/// </summary>
/// <remarks>
/// 不变式（由 <see cref="DepartmentManager"/> 保证）：
/// 1. 名称、编码必填且不超过约定长度；
/// 2. 编码在同一租户内唯一；
/// 3. 上级部门必须存在，且不能是自身或其子孙节点；
/// 4. 存在子部门时不允许删除。
/// 实现 <see cref="IMultiTenant"/>，由 ABP 自动完成租户过滤。
/// </remarks>
public class Department : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 所属租户 Id，host 数据为 null；由 ABP 自动维护。
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 部门名称，必填，长度见 <see cref="DepartmentConsts.MaxNameLength"/>。
    /// </summary>
    public virtual string Name { get; protected set; } = default!;

    /// <summary>
    /// 部门编码，业务侧唯一标识，长度见 <see cref="DepartmentConsts.MaxCodeLength"/>。
    /// </summary>
    public virtual string Code { get; protected set; } = default!;

    /// <summary>
    /// 上级部门 Id；为 null 表示一级部门。
    /// </summary>
    public virtual Guid? ParentId { get; protected set; }

    /// <summary>
    /// 部门负责人姓名，可为空。
    /// </summary>
    public virtual string? Leader { get; protected set; }

    /// <summary>
    /// 部门联系电话，可为空。
    /// </summary>
    public virtual string? PhoneNumber { get; protected set; }

    /// <summary>
    /// 同级排序号，值越小越靠前。
    /// </summary>
    public virtual int Sort { get; protected set; }

    /// <summary>
    /// 是否启用；停用部门不参与业务下拉选择。
    /// </summary>
    public virtual bool IsActive { get; protected set; }

    /// <summary>
    /// 备注说明，可为空。
    /// </summary>
    public virtual string? Remark { get; protected set; }

    /// <summary>
    /// 供 ORM 反序列化使用的无参构造，禁止业务代码调用。
    /// </summary>
    protected Department()
    {
    }

    /// <summary>
    /// 创建部门实例。
    /// </summary>
    /// <param name="id">聚合根 Id，通常由 <c>GuidGenerator</c> 生成。</param>
    /// <param name="name">部门名称。</param>
    /// <param name="code">部门编码（租户内唯一）。</param>
    /// <param name="parentId">上级部门 Id，可为空表示一级部门。</param>
    /// <param name="leader">部门负责人，可为空。</param>
    /// <param name="phoneNumber">联系电话，可为空。</param>
    /// <param name="sort">排序号。</param>
    /// <param name="remark">备注，可为空。</param>
    /// <param name="isActive">是否启用，默认启用。</param>
    public Department(
        Guid id,
        string name,
        string code,
        Guid? parentId = null,
        string? leader = null,
        string? phoneNumber = null,
        int sort = 0,
        string? remark = null,
        bool isActive = true)
        : base(id)
    {
        SetName(name);
        SetCode(code);
        SetParent(parentId);
        SetLeader(leader);
        SetPhoneNumber(phoneNumber);
        SetRemark(remark);
        Sort = sort;
        IsActive = isActive;
    }

    /// <summary>
    /// 修改部门名称，校验必填与长度。
    /// </summary>
    /// <param name="name">新的部门名称。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), DepartmentConsts.MaxNameLength);

        Name = name;
        return this;
    }

    /// <summary>
    /// 修改部门编码，校验必填与长度（唯一性由 <see cref="DepartmentManager"/> 校验）。
    /// </summary>
    /// <param name="code">新的部门编码。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), DepartmentConsts.MaxCodeLength);

        Code = code;
        return this;
    }

    /// <summary>
    /// 设置上级部门；合法性（存在性、非自身子孙）由 <see cref="DepartmentManager"/> 校验。
    /// </summary>
    /// <param name="parentId">上级部门 Id，可为空表示挂到根级。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetParent(Guid? parentId)
    {
        ParentId = parentId;
        return this;
    }

    /// <summary>
    /// 设置部门负责人。
    /// </summary>
    /// <param name="leader">负责人姓名，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetLeader(string? leader)
    {
        Leader = leader;
        return this;
    }

    /// <summary>
    /// 设置部门联系电话。
    /// </summary>
    /// <param name="phoneNumber">电话号码，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
        return this;
    }

    /// <summary>
    /// 设置部门备注。
    /// </summary>
    /// <param name="remark">备注内容，可为空。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetRemark(string? remark)
    {
        Remark = remark;
        return this;
    }

    /// <summary>
    /// 设置排序号。
    /// </summary>
    /// <param name="sort">排序号，值越小越靠前。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetSort(int sort)
    {
        Sort = sort;
        return this;
    }

    /// <summary>
    /// 启用或停用部门。
    /// </summary>
    /// <param name="isActive">true 为启用。</param>
    /// <returns>当前实体，便于链式调用。</returns>
    public virtual Department SetActive(bool isActive)
    {
        IsActive = isActive;
        return this;
    }
}
