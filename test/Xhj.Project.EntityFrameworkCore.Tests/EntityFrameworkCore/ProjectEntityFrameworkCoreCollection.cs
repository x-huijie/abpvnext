using Xunit;

namespace Xhj.Project.EntityFrameworkCore;

[CollectionDefinition(ProjectTestConsts.CollectionDefinitionName)]
public class ProjectEntityFrameworkCoreCollection : ICollectionFixture<ProjectEntityFrameworkCoreFixture>
{

}
