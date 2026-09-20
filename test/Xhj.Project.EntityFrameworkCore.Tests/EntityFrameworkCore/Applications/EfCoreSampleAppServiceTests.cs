using Xhj.Project.Samples;
using Xunit;

namespace Xhj.Project.EntityFrameworkCore.Applications;

[Collection(ProjectTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ProjectEntityFrameworkCoreTestModule>
{

}
