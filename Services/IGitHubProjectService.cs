using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Service for managing synchronized GitHub Projects.
/// </summary>
public interface IGitHubProjectService
{
    /// <summary>Get projects for a repository.</summary>
    Task<List<GitHubProjectDto>> GetByRepositoryAsync(int repositoryId, int moduleId);

    /// <summary>Sync projects from GitHub API.</summary>
    Task<int> SyncProjectsAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
