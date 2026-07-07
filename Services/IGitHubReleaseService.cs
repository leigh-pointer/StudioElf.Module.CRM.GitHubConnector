using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Service for managing synchronized GitHub releases.
/// </summary>
public interface IGitHubReleaseService
{
    /// <summary>Get all releases for a specific repository.</summary>
    Task<List<GitHubReleaseDto>> GetByRepositoryAsync(int repositoryId, int moduleId);

    /// <summary>Get the most recent releases across all tracked repositories.</summary>
    Task<List<GitHubReleaseDto>> GetRecentAsync(int moduleId, int count = 10);

    /// <summary>Sync releases for a repository from GitHub API.</summary>
    /// <returns>The number of releases synced (created or updated).</returns>
    Task<int> SyncReleasesAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
