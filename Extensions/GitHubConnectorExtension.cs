using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using StudioElf.Module.CRM.Models;
using StudioElf.Module.CRM.Services;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Services;

namespace StudioElf.Module.CRM.GitHubConnector;

/// <summary>
/// ICrmExtension contract for the GitHub Enterprise Connector.
/// Registered as singleton via factory: <c>sp =&gt; new GitHubConnectorExtension()</c>.
/// Timeline queries use service locator via <see cref="Initialize"/>.
/// </summary>
public class GitHubConnectorExtension : ICrmExtension
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initialize service locator. Called once from <c>ServerStartup.ConfigureServices</c>
    /// after all services are registered.
    /// </summary>
    internal static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public string ExtensionId => GitHubConnectorModuleInfo.ExtensionId;

    /// <inheritdoc />
    public string DisplayName => GitHubConnectorModuleInfo.DisplayName;

    /// <inheritdoc />
    public string Description => GitHubConnectorModuleInfo.Description;

    /// <inheritdoc />
    public string Version => GitHubConnectorModuleInfo.Version;

    /// <inheritdoc />
    public string IconClass => GitHubConnectorModuleInfo.IconClass;

    /// <inheritdoc />
    public List<CrmNavItem> GetNavItems() => new()
    {
        new("github", "GitHub", "bi bi-github", 100),
    };

    /// <inheritdoc />
    public List<CrmDashboardWidget> GetDashboardWidgets() => new()
    {
        new("github-overview", "GitHub Overview", typeof(GitHubOverviewWidget), 10),
        new("github-recent-releases", "Recent Releases", typeof(GitHubRecentReleasesWidget), 20),
        new("github-analytics", "GitHub Analytics", typeof(GitHubAnalyticsWidget), 30),
    };

    /// <inheritdoc />
    public List<CrmContactTab> GetContactTabs() => new()
    {
        new("github-repos", "GitHub Repos", typeof(GitHubContactTab), 50),
        new("github-knowledge-graph", "Knowledge Graph", typeof(GitHubKnowledgeGraphTab), 60),
    };

    /// <inheritdoc />
    public List<CrmEmailTemplate> GetEmailTemplates() => new()
    {
        new("Release Published", "New Release: {{Release.TagName}} for {{Repository.FullName}}",
            "<p>Hi {{Contact.FirstName}},</p><p>A new release <strong>{{Release.TagName}}</strong> has been published for <strong>{{Repository.FullName}}</strong>.</p><p>{{Release.Body}}</p><p><a href='{{Release.Url}}'>View on GitHub</a></p>"),
        new("Sync Completed", "GitHub Sync Completed — {{SyncResult.Message}}",
            "<p>GitHub synchronization completed.</p><p>{{SyncResult.RepositoriesUpdated}} repos, {{SyncResult.ReleasesUpdated}} releases, {{SyncResult.IssuesUpdated}} issues synced.</p>"),
    };

    /// <inheritdoc />
    public Type GetShellComponentType() => typeof(GitHubConnectorShell);
    public Type GetSettingsComponentType() => typeof(GitHubHostSettings);
    public Type GetUserSettingsComponentType() => typeof(GitHubUserSettings);

    /// <inheritdoc />
    public List<TimelineItem> GetTimelineItems(string entityName, int entityId, int moduleId, TimelineFilter filter)
    {
        if (_serviceProvider == null)
            return new();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var timelineService = scope.ServiceProvider.GetRequiredService<IGitHubTimelineService>();
            return timelineService.GetTimelineItemsAsync(entityName, entityId, moduleId, filter)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception)
        {
            return new();
        }
    }
}
