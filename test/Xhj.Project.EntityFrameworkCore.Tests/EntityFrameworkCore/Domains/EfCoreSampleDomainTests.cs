using Xhj.Project.Samples;
using Xunit;

namespace Xhj.Project.EntityFrameworkCore.Domains;

[Collection(ProjectTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ProjectEntityFrameworkCoreTestModule>
{

}
