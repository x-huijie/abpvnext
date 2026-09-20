using Volo.Abp.Modularity;

namespace Xhj.Project;

[DependsOn(
    typeof(ProjectDomainModule),
    typeof(ProjectTestBaseModule)
)]
public class ProjectDomainTestModule : AbpModule
{

}
