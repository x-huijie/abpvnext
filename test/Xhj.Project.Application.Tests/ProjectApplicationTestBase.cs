using Volo.Abp.Modularity;

namespace Xhj.Project;

public abstract class ProjectApplicationTestBase<TStartupModule> : ProjectTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
