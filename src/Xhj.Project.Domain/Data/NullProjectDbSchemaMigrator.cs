using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Xhj.Project.Data;

/* This is used if database provider does't define
 * IProjectDbSchemaMigrator implementation.
 */
public class NullProjectDbSchemaMigrator : IProjectDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
