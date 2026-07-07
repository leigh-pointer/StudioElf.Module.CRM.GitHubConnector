using Oqtane.Models;
using Oqtane.Modules;
using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector;

public class ModuleInfo : IModule
{
    public ModuleDefinition ModuleDefinition => new ModuleDefinition
    {
        Name = GitHubConnectorModuleInfo.DisplayName,
        Description = GitHubConnectorModuleInfo.Description,
        Categories = "Headless",
        Version = GitHubConnectorModuleInfo.Version,
        ReleaseVersions = GitHubConnectorModuleInfo.Version,
        ServerManagerType = "StudioElf.Module.GitHubConnector.Manager.GitHubConnectorManager, StudioElf.Module.CRM.GitHubConnector.Oqtane",
        Dependencies = "StudioElf.Module.CRM.Shared.Oqtane",
        PackageName = "StudioElf.Module.CRM.GitHubConnector"
    };
}
