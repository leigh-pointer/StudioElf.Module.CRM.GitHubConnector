using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

public interface IGitHubDiscussionService
{
    Task<List<GitHubDiscussionDto>> GetByRepositoryAsync(int repositoryId, int moduleId);
    Task<int> SyncDiscussionsAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
