using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Manager;

public class GitHubConnectorManager : MigratableModuleBase, IInstallable
{
    private readonly IDBContextDependencies _DBContextDependencies;

    public GitHubConnectorManager(IDBContextDependencies DBContextDependencies)
    {
        _DBContextDependencies = DBContextDependencies;
    }

    public bool Install(Tenant tenant, string version)
    {
        return Migrate(new GitHubConnectorContext(_DBContextDependencies), tenant, MigrationType.Up);
    }

    public bool Uninstall(Tenant tenant)
    {
        return Migrate(new GitHubConnectorContext(_DBContextDependencies), tenant, MigrationType.Down);
    }
}
