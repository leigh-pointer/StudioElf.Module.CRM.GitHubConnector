using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Default implementation of <see cref="IGitHubReleaseService"/>.
/// Manages release synchronization and querying.
/// </summary>
public class GitHubReleaseService : IGitHubReleaseService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _contextFactory;
    private readonly IGitHubApiClient _apiClient;
    private readonly ILogger<GitHubReleaseService> _logger;

    public GitHubReleaseService(
        IDbContextFactory<GitHubConnectorContext> contextFactory,
        IGitHubApiClient apiClient,
        ILogger<GitHubReleaseService> logger)
    {
        _contextFactory = contextFactory;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<GitHubReleaseDto>> GetByRepositoryAsync(int repositoryId, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.GitHubReleases
            .Where(r => r.RepositoryId == repositoryId && r.Repository.ModuleId == moduleId)
            .Include(r => r.Repository)
            .OrderByDescending(r => r.PublishedAt)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<List<GitHubReleaseDto>> GetRecentAsync(int moduleId, int count = 10)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.GitHubReleases
            .Where(r => r.Repository.ModuleId == moduleId)
            .OrderByDescending(r => r.PublishedAt)
            .Take(count)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<List<GitHubReleaseDto>> GetByEntityAsync(string entityType, int entityId, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var repoIds = await db.GitHubRepositoryLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .Select(l => l.RepositoryId)
            .ToListAsync();

        if (repoIds.Count == 0) return new();

        return await db.GitHubReleases
            .Where(r => repoIds.Contains(r.RepositoryId) && r.Repository.ModuleId == moduleId)
            .OrderByDescending(r => r.PublishedAt)
            .Take(50)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<int> SyncReleasesAsync(int repositoryId, int moduleId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var repo = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.Id == repositoryId && r.ModuleId == moduleId, ct);

        if (repo == null)
            throw new KeyNotFoundException($"Repository {repositoryId} not found in module {moduleId}.");

        var parts = repo.FullName?.Split('/');
        if (parts == null || parts.Length != 2)
            throw new InvalidOperationException($"Repository '{repo.FullName}' has an invalid full name format.");

        var releases = await _apiClient.GetReleasesAsync(parts[0], parts[1], ct);
        var syncedCount = 0;

        foreach (var releaseJson in releases)
        {
            ct.ThrowIfCancellationRequested();
            var root = releaseJson.RootElement;

            var releaseId = root.GetProperty("id").GetInt64();
            var tagName = root.TryGetProperty("tag_name", out var tag) ? tag.GetString() : null;

            var existing = await db.GitHubReleases
                .FirstOrDefaultAsync(r => r.ReleaseId == releaseId && r.RepositoryId == repositoryId, ct);

            if (existing == null)
            {
                existing = new GitHubRelease
                {
                    RepositoryId = repositoryId,
                    ReleaseId = releaseId,
                    CreatedOn = DateTime.UtcNow,
                };
                db.GitHubReleases.Add(existing);
            }

            existing.TagName = tagName;
            existing.ReleaseName = root.TryGetProperty("name", out var name) ? name.GetString() : null;
            existing.Body = root.TryGetProperty("body", out var body) ? body.GetString() : null;
            existing.Url = root.TryGetProperty("html_url", out var htmlUrl) ? htmlUrl.GetString() : null;
            existing.IsPrerelease = root.TryGetProperty("prerelease", out var prerelease) && prerelease.GetBoolean();
            existing.PublishedAt = root.TryGetProperty("published_at", out var published) && published.ValueKind == JsonValueKind.String
                ? DateTime.Parse(published.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                : DateTime.UtcNow;
            existing.ModifiedOn = DateTime.UtcNow;

            syncedCount++;
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Synced {Count} releases for repository {RepoId} (module {ModuleId})",
            syncedCount, repositoryId, moduleId);

        return syncedCount;
    }

    private static GitHubReleaseDto ToDto(GitHubRelease entity)
    {
        return new GitHubReleaseDto
        {
            Id = entity.Id,
            RepositoryId = entity.RepositoryId,
            RepositoryName = entity.Repository?.FullName ?? string.Empty,
            ReleaseId = entity.ReleaseId,
            TagName = entity.TagName,
            ReleaseName = entity.ReleaseName,
            Body = entity.Body,
            Url = entity.Url,
            IsPrerelease = entity.IsPrerelease,
            PublishedAt = entity.PublishedAt,
        };
    }
}
