using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Shared;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Background hosted service that periodically synchronizes GitHub repository data.
/// Auto-registers with the Oqtane Job Scheduler on first run.
/// Enabled only when a Personal Access Token is configured.
/// </summary>
public class GitHubSyncHostedService : HostedServiceBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="GitHubSyncHostedService"/>.
    /// </summary>
    public GitHubSyncHostedService(IServiceScopeFactory serviceScopeFactory)
        : base(serviceScopeFactory)
    {
        Name = "GitHub Connector Sync";
        Frequency = "m";
        Interval = 30;
        IsEnabled = false;
        MaximumDuration = 30;
    }

    /// <inheritdoc />
    public override async Task<string> ExecuteJobAsync(IServiceProvider provider)
    {
        var logger = provider.GetRequiredService<ILogger<GitHubSyncHostedService>>();
        var syncService = provider.GetRequiredService<IGitHubSyncService>();
        var moduleRepository = provider.GetRequiredService<IModuleRepository>();
        var aliasRepository = provider.GetRequiredService<IAliasRepository>();

        var results = new List<string>();

        try
        {
            // Get all aliases (sites) for the current tenant
            var aliases = aliasRepository.GetAliases().ToList();
            if (aliases.Count == 0)
            {
                logger.LogDebug("GitHub Sync: No aliases found for tenant.");
                return "No aliases found.";
            }

            foreach (var alias in aliases)
            {
                // Find modules with GitHubConnector ModuleDefinitionName
                var modules = moduleRepository.GetModules(alias.SiteId)
                    ?.Where(m => m.ModuleDefinitionName?.Contains("GitHubConnector") == true)
                    .ToList() ?? new();

                if (modules.Count == 0)
                    continue;

                foreach (var module in modules)
                {
                    logger.LogInformation(
                        "GitHub Sync: Starting sync for module {ModuleId} on site {SiteId}",
                        module.ModuleId, module.SiteId);

                    var result = await syncService.SyncAllAsync(module.ModuleId);
                    results.Add($"Module {module.ModuleId}: {result.Message}");
                    logger.LogInformation("GitHub Sync: {Message}", result.Message);
                }
            }

            return results.Count > 0
                ? string.Join(" | ", results)
                : "No GitHub Connector modules found.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GitHub Sync job failed");
            return $"GitHub Sync failed: {ex.Message}";
        }
    }
}
