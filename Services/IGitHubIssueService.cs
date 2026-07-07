using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Service for managing synchronized GitHub issues and pull requests.
/// </summary>
public interface IGitHubIssueService
{
    /// <summary>Get issues for a repository, optionally filtered by state.</summary>
    Task<List<GitHubIssueDto>> GetByRepositoryAsync(int repositoryId, int moduleId, string? state = null);

    /// <summary>Get open issues for all linked repos of a CRM entity.</summary>
    Task<List<GitHubIssueDto>> GetByEntityAsync(string entityType, int entityId, int moduleId, bool pullRequests = false);

    /// <summary>Sync issues for a repository from GitHub API.</summary>
    Task<int> SyncIssuesAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
