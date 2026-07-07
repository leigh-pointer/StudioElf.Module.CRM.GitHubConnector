using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.CRM.Models;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Service that returns GitHub timeline items for CRM entities (Contact, Company, Deal).
/// Called from <see cref="GitHubConnectorExtension.GetTimelineItems"/> via service locator.
/// </summary>
public interface IGitHubTimelineService
{
    /// <summary>Get GitHub timeline items for a CRM entity.</summary>
    Task<List<TimelineItem>> GetTimelineItemsAsync(string entityName, int entityId, int moduleId, TimelineFilter filter);
}

public class GitHubTimelineService : IGitHubTimelineService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _contextFactory;
    private readonly ILogger<GitHubTimelineService> _logger;

    public GitHubTimelineService(
        IDbContextFactory<GitHubConnectorContext> contextFactory,
        ILogger<GitHubTimelineService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<TimelineItem>> GetTimelineItemsAsync(string entityName, int entityId, int moduleId, TimelineFilter filter)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        // Get repos linked to this entity
        var repoIds = await db.GitHubRepositoryLinks
            .Where(l => l.EntityType == entityName.ToLowerInvariant() && l.EntityId == entityId)
            .Select(l => l.RepositoryId)
            .ToListAsync();

        if (repoIds.Count == 0)
            return new();

        var items = new List<TimelineItem>();

        // Add synced releases for linked repos
        var releases = await db.GitHubReleases
            .Where(r => repoIds.Contains(r.RepositoryId))
            .OrderByDescending(r => r.PublishedAt)
            .Take(20)
            .ToListAsync();

        foreach (var release in releases)
        {
            items.Add(new TimelineItem
            {
                SourceType = "GitHubConnector",
                SourceId = release.Id,
                EntityName = entityName,
                EntityId = entityId,
                ItemType = "github.release",
                Summary = $"Release {release.TagName} published for {release.Repository?.FullName ?? "unknown"}",
                DetailMarkdown = release.Body,
                Icon = "bi bi-tag",
                Color = "primary",
                OccurredOn = release.PublishedAt,
                ExtensionId = "GitHubConnector",
                NavigationUrl = release.Url,
            });
        }

        // Add synced issues for linked repos
        var issues = await db.GitHubIssues
            .Where(i => repoIds.Contains(i.RepositoryId) && i.State == "open")
            .OrderByDescending(i => i.CreatedAt)
            .Take(20)
            .ToListAsync();

        foreach (var issue in issues)
        {
            items.Add(new TimelineItem
            {
                SourceType = "GitHubConnector",
                SourceId = issue.Id,
                EntityName = entityName,
                EntityId = entityId,
                ItemType = issue.IsPullRequest ? "github.pullrequest" : "github.issue",
                Summary = issue.IsPullRequest
                    ? $"PR #{issue.IssueNumber}: {issue.Title}"
                    : $"Issue #{issue.IssueNumber}: {issue.Title}",
                DetailMarkdown = issue.Body,
                Icon = issue.IsPullRequest ? "bi bi-git-pull-request" : "bi bi-bug",
                Color = issue.IsPullRequest ? "success" : "warning",
                OccurredOn = issue.CreatedAt,
                ExtensionId = "GitHubConnector",
                NavigationUrl = issue.HtmlUrl,
            });
        }

        // Apply filter
        if (filter.ItemTypes?.Count > 0)
        {
            items = items.Where(i => filter.ItemTypes.Contains(i.ItemType)).ToList();
        }

        return items
            .OrderByDescending(i => i.OccurredOn)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();
    }
}
