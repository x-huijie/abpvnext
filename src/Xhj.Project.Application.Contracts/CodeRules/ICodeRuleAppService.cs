using System;
using Volo.Abp.Application.Services;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码规则应用服务：维护单号生成规则。
/// </summary>
/// <remarks>
/// 单号的实际生成请使用 <see cref="ICodeGeneratorAppService"/>，不要自行拼接流水号。
/// </remarks>
public interface ICodeRuleAppService :
    ICrudAppService<CodeRuleDto, Guid, GetCodeRuleListInput, CreateUpdateCodeRuleDto>
{
}
