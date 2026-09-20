using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Xhj.Project.Permissions;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则应用服务：规则的新增、编辑、删除与查询。
/// </summary>
[Authorize(ProjectPermissions.CodeRules.Default)]
public class CodeRuleAppService :
    CrudAppService<CodeRule, CodeRuleDto, Guid, GetCodeRuleListInput, CreateUpdateCodeRuleDto>,
    ICodeRuleAppService
{
    /// <summary>
    /// 构造编码规则应用服务。
    /// </summary>
    /// <param name="repository">编码规则仓储。</param>
    public CodeRuleAppService(IRepository<CodeRule, Guid> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 新增规则。
    /// </summary>
    /// <param name="input">规则信息。</param>
    /// <returns>新建的规则。</returns>
    /// <exception cref="BusinessException">规则标识重复时抛出。</exception>
    [Authorize(ProjectPermissions.CodeRules.Create)]
    public override async Task<CodeRuleDto> CreateAsync(CreateUpdateCodeRuleDto input)
    {
        var exists = await Repository.AnyAsync(x => x.Code == input.Code);

        if (exists)
        {
            throw new BusinessException(ProjectDomainErrorCodes.CodeRuleCodeAlreadyExists)
                .WithData("Code", input.Code);
        }

        var rule = new CodeRule(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Prefix,
            input.DateFormat,
            input.Separator,
            input.SerialLength,
            input.Step,
            input.ResetMode,
            input.IsEnabled);

        await Repository.InsertAsync(rule, autoSave: true);

        return ObjectMapper.Map<CodeRule, CodeRuleDto>(rule);
    }

    /// <summary>
    /// 编辑规则配置。
    /// </summary>
    /// <param name="id">规则 Id。</param>
    /// <param name="input">新的规则信息。</param>
    /// <returns>更新后的规则。</returns>
    [Authorize(ProjectPermissions.CodeRules.Update)]
    public override async Task<CodeRuleDto> UpdateAsync(Guid id, CreateUpdateCodeRuleDto input)
    {
        var rule = await Repository.GetAsync(id);

        rule
            .SetName(input.Name)
            .SetPrefix(input.Prefix)
            .SetDateFormat(input.DateFormat)
            .SetSeparator(input.Separator)
            .SetSerialLength(input.SerialLength)
            .SetStep(input.Step)
            .SetResetMode(input.ResetMode)
            .SetEnabled(input.IsEnabled);

        // 标识是业务侧的取号依据，一经使用不再允许变更
        await Repository.UpdateAsync(rule, autoSave: true);

        return ObjectMapper.Map<CodeRule, CodeRuleDto>(rule);
    }

    /// <summary>
    /// 删除规则。
    /// </summary>
    /// <param name="id">规则 Id。</param>
    [Authorize(ProjectPermissions.CodeRules.Delete)]
    public override Task DeleteAsync(Guid id)
    {
        return Repository.DeleteAsync(id, autoSave: true);
    }

    /// <summary>
    /// 构造列表查询条件。
    /// </summary>
    /// <param name="input">查询入参。</param>
    /// <returns>已应用过滤条件的查询对象。</returns>
    protected override async Task<IQueryable<CodeRule>> CreateFilteredQueryAsync(GetCodeRuleListInput input)
    {
        var query = await Repository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Code.Contains(input.Filter!) || x.Name.Contains(input.Filter!));
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        return query;
    }
}
