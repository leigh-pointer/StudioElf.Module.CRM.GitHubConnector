using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>Aggregated analytics for tracked GitHub repositories.</summary>
public interface IGitHubAnalyticsService
{
    /// <summary>Compute analytics for a module.</summary>
    Task<GitHubAnalyticsDto> GetAnalyticsAsync(int moduleId);
}
