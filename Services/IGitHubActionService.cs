using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>Sync and query GitHub Actions workflow runs.</summary>
public interface IGitHubActionService
{
    /// <summary>Get workflow runs for a repository.</summary>
    Task<List<GitHubActionWorkflowDto>> GetByRepositoryAsync(int repositoryId, int moduleId);
    /// <summary>Sync workflow runs from GitHub API.</summary>
    Task<int> SyncActionsAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
