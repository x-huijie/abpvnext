using Volo.Abp.Modularity;

namespace Xhj.Project;

[DependsOn(
    typeof(ProjectApplicationModule),
    typeof(ProjectDomainTestModule)
)]
public class ProjectApplicationTestModule : AbpModule
{

}
