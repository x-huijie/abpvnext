using System.Threading.Tasks;

namespace Xhj.Project.Data;

public interface IProjectDbSchemaMigrator
{
    Task MigrateAsync();
}
