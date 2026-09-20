using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.DependencyInjection;
using Xhj.Project.Permissions;

namespace Xhj.Project.CodeRules;

/// <summary>
/// 编码生成器应用服务：对外提供统一的取号入口。
/// </summary>
/// <remarks>
/// 只做转发，生成规则与并发控制全部在领域服务 <see cref="CodeGenerator"/> 中。
/// </remarks>
[Authorize(ProjectPermissions.CodeRules.Default)]
public class CodeGeneratorAppService : ProjectAppService, ICodeGeneratorAppService, ITransientDependency
{
    private readonly CodeGenerator _codeGenerator;

    /// <summary>
    /// 构造编码生成器应用服务。
    /// </summary>
    /// <param name="codeGenerator">编码生成器领域服务。</param>
    public CodeGeneratorAppService(CodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator;
    }

    /// <summary>
    /// 生成一个业务单号。
    /// </summary>
    /// <param name="ruleCode">规则标识，如 "Order"。</param>
    /// <returns>单号。</returns>
    public virtual Task<string> GenerateAsync(string ruleCode)
    {
        return _codeGenerator.GenerateAsync(ruleCode);
    }

    /// <summary>
    /// 批量生成连续的业务单号。
    /// </summary>
    /// <param name="ruleCode">规则标识。</param>
    /// <param name="count">生成数量。</param>
    /// <returns>单号列表。</returns>
    public virtual Task<List<string>> GenerateBatchAsync(string ruleCode, int count)
    {
        return _codeGenerator.GenerateAsync(ruleCode, count);
    }
}
