using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

public interface IGitHubProjectService
{
    Task<List<GitHubProjectDto>> GetByRepositoryAsync(int repositoryId, int moduleId);
    Task<int> SyncProjectsAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
