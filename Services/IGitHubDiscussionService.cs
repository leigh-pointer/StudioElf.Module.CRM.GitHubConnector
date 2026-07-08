using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Service for managing synchronized GitHub Discussions.
/// </summary>
public interface IGitHubDiscussionService
{
    /// <summary>Get discussions for a repository.</summary>
    Task<List<GitHubDiscussionDto>> GetByRepositoryAsync(int repositoryId, int moduleId);

    /// <summary>Sync discussions from GitHub API.</summary>
    Task<int> SyncDiscussionsAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
