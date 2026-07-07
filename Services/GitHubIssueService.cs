using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubIssueService : IGitHubIssueService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _contextFactory;
    private readonly IGitHubApiClient _apiClient;
    private readonly ILogger<GitHubIssueService> _logger;

    public GitHubIssueService(
        IDbContextFactory<GitHubConnectorContext> contextFactory,
        IGitHubApiClient apiClient,
        ILogger<GitHubIssueService> logger)
    {
        _contextFactory = contextFactory;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<GitHubIssueDto>> GetByRepositoryAsync(int repositoryId, int moduleId, string? state = null)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var query = db.GitHubIssues
            .Where(i => i.RepositoryId == repositoryId && i.Repository.ModuleId == moduleId);
        if (!string.IsNullOrEmpty(state))
            query = query.Where(i => i.State == state);
        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => ToDto(i))
            .ToListAsync();
    }

    public async Task<List<GitHubIssueDto>> GetByEntityAsync(string entityType, int entityId, int moduleId, bool pullRequests = false)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var repoIds = db.GitHubRepositoryLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .Select(l => l.RepositoryId);

        var query = db.GitHubIssues
            .Where(i => repoIds.Contains(i.RepositoryId) && i.State == "open");
        if (pullRequests)
            query = query.Where(i => i.IsPullRequest);
        else
            query = query.Where(i => !i.IsPullRequest);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .Select(i => ToDto(i))
            .ToListAsync();
    }

    public async Task<int> SyncIssuesAsync(int repositoryId, int moduleId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var repo = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.Id == repositoryId && r.ModuleId == moduleId, ct);
        if (repo == null)
            throw new KeyNotFoundException($"Repository {repositoryId} not found in module {moduleId}.");

        var parts = repo.FullName?.Split('/');
        if (parts == null || parts.Length != 2)
            throw new InvalidOperationException($"Repository '{repo.FullName}' has invalid format.");

        var issues = await _apiClient.GetIssuesAsync(parts[0], parts[1], "all", ct);
        var syncedCount = 0;

        foreach (var issueJson in issues)
        {
            ct.ThrowIfCancellationRequested();
            var root = issueJson.RootElement;

            var issueNumber = root.GetProperty("number").GetInt64();
            var isPr = root.TryGetProperty("pull_request", out _);

            var existing = await db.GitHubIssues
                .FirstOrDefaultAsync(i => i.IssueNumber == issueNumber && i.RepositoryId == repositoryId, ct);

            if (existing == null)
            {
                existing = new GitHubIssue
                {
                    RepositoryId = repositoryId,
                    IssueNumber = issueNumber,
                    CreatedOn = DateTime.UtcNow,
                };
                db.GitHubIssues.Add(existing);
            }

            existing.Title = root.GetProperty("title").GetString() ?? "";
            existing.Body = root.TryGetProperty("body", out var body) ? body.GetString() : null;
            existing.State = root.GetProperty("state").GetString() ?? "open";
            existing.Url = root.TryGetProperty("url", out var url) ? url.GetString() : null;
            existing.HtmlUrl = root.TryGetProperty("html_url", out var htmlUrl) ? htmlUrl.GetString() : null;
            existing.UserLogin = root.TryGetProperty("user", out var user) ? user.GetProperty("login").GetString() : null;
            existing.IsPullRequest = isPr;

            if (root.TryGetProperty("labels", out var labels) && labels.ValueKind == JsonValueKind.Array)
            {
                var labelNames = labels.EnumerateArray().Select(l => l.GetProperty("name").GetString()).Where(n => n != null);
                existing.Labels = string.Join(",", labelNames);
            }

            existing.CreatedAt = DateTime.Parse(root.GetProperty("created_at").GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            existing.UpdatedAt = root.TryGetProperty("updated_at", out var updated) && updated.ValueKind == JsonValueKind.String
                ? DateTime.Parse(updated.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                : null;
            existing.ClosedAt = root.TryGetProperty("closed_at", out var closed) && closed.ValueKind == JsonValueKind.String
                ? DateTime.Parse(closed.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                : null;
            existing.ModifiedOn = DateTime.UtcNow;

            syncedCount++;
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Synced {Count} issues for repo {RepoId}", syncedCount, repositoryId);
        return syncedCount;
    }

    private static GitHubIssueDto ToDto(GitHubIssue entity)
    {
        return new GitHubIssueDto
        {
            Id = entity.Id,
            RepositoryId = entity.RepositoryId,
            RepositoryName = entity.Repository?.FullName ?? "",
            IssueNumber = entity.IssueNumber,
            Title = entity.Title,
            Body = entity.Body,
            State = entity.State,
            Url = entity.Url,
            HtmlUrl = entity.HtmlUrl,
            Labels = entity.Labels,
            UserLogin = entity.UserLogin,
            IsPullRequest = entity.IsPullRequest,
            MergeState = entity.MergeState,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ClosedAt = entity.ClosedAt,
        };
    }
}
