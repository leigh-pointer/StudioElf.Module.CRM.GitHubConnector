using System;
using System.Collections.Generic;
using StudioElf.Module.CRM.Models;
using StudioElf.Module.CRM.Services;
using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.CRM.GitHubConnector;

/// <summary>
/// ICrmExtension contract for the GitHub Enterprise Connector.
/// Registered as singleton via factory: <c>sp =&gt; new GitHubConnectorExtension()</c>.
/// No constructor DI available — all values from <see cref="GitHubConnectorModuleInfo"/> constants.
/// </summary>
public class GitHubConnectorExtension : ICrmExtension
{
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
    /// <remarks>
    /// Phase 1: Nav item added for CRM navigation.
    /// Phase 2+: Additional items for issues, PRs, analytics.
    /// </remarks>
    public List<CrmNavItem> GetNavItems() => new()
    {
        new("github", "GitHub", "bi bi-github", 100),
    };

    /// <inheritdoc />
    public List<CrmDashboardWidget> GetDashboardWidgets() => new()
    {
        new("github-overview", "GitHub Overview", typeof(GitHubOverviewWidget), 10),
        new("github-recent-releases", "Recent Releases", typeof(GitHubRecentReleasesWidget), 20),
    };

    /// <inheritdoc />
    /// <remarks>
    /// Adds a "GitHub Repos" tab to contact detail views.
    /// Phase 2 will add tabs for Companies and Deals.
    /// </remarks>
    public List<CrmContactTab> GetContactTabs() => new()
    {
        new("github-repos", "GitHub Repos", typeof(GitHubContactTab), 50),
    };

    /// <inheritdoc />
    /// <remarks>Email templates planned for Phase 2+.</remarks>
    public List<CrmEmailTemplate> GetEmailTemplates() => new();

    /// <inheritdoc />
    public Type GetShellComponentType() => typeof(GitHubConnectorShell);

    /// <inheritdoc />
    /// <remarks>
    /// Phase 1: Returns null. Timeline events are written during sync via
    /// <c>ICrmContactService.LogCommunicationAsync()</c>.
    /// Phase 2 will implement proper timeline querying via service locator pattern.
    /// </remarks>
    public List<TimelineItem> GetTimelineItems(string entityName, int entityId, int moduleId, TimelineFilter filter)
        => new();
}
