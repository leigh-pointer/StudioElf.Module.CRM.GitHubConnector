using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Orchestration service that coordinates full synchronization of all
/// tracked GitHub repositories and their releases.
/// Called by both the background job (<see cref="GitHubSyncHostedService"/>)
/// and the manual sync API endpoint.
/// </summary>
public interface IGitHubSyncService
{
    /// <summary>Sync all repositories and their releases for a module.</summary>
    Task<SyncResultDto> SyncAllAsync(int moduleId, CancellationToken ct = default);
}
