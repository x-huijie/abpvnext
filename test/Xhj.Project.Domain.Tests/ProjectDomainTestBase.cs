using Volo.Abp.Modularity;

namespace Xhj.Project;

/* Inherit from this class for your domain layer tests. */
public abstract class ProjectDomainTestBase<TStartupModule> : ProjectTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
