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
    /// <param name="moduleId">CRM module ID.</param>
    /// <param name="settingsJson">Optional pre-loaded settings JSON. If null, loads from ISettingService.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SyncResultDto> SyncAllAsync(int moduleId, string? patOverride = null, CancellationToken ct = default);
}
