using System.Text.Json;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// HTTP client wrapper for the GitHub REST API v3.
/// Handles authentication, pagination, and rate-limit awareness.
/// </summary>
/// <remarks>
/// Configure with <see cref="Configure"/> before making API calls.
/// This allows settings to change at runtime (e.g., via module settings UI)
/// without recreating the service.
/// </remarks>
public interface IGitHubApiClient
{
    /// <summary>Fetch a single repository by owner and name.</summary>
    /// <param name="owner">Repository owner (user or organization).</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Parsed GitHub API response as <see cref="JsonDocument"/>.</returns>
    Task<JsonDocument> GetRepositoryAsync(string owner, string repo, CancellationToken ct = default);

    /// <summary>Fetch all releases for a repository (handles pagination).</summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of parsed GitHub API release responses.</returns>
    Task<List<JsonDocument>> GetReleasesAsync(string owner, string repo, CancellationToken ct = default);

    /// <summary>Verify the Personal Access Token is valid by calling GET /user.</summary>
    Task<bool> ValidateTokenAsync(CancellationToken ct = default);

    /// <summary>Fetch issues and pull requests for a repository.</summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="state">Filter by state: "open", "closed", "all". Default "open".</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of parsed GitHub API issue responses.</returns>
    Task<List<JsonDocument>> GetIssuesAsync(string owner, string repo, string state = "open", CancellationToken ct = default);

    /// <summary>Fetch workflow runs for a repository.</summary>
    Task<List<JsonDocument>> GetActionsAsync(string owner, string repo, CancellationToken ct = default);

    /// <summary>Fetch discussions for a repository.</summary>
    Task<List<JsonDocument>> GetDiscussionsAsync(string owner, string repo, CancellationToken ct = default);

    /// <summary>Fetch projects (classic) for a repository.</summary>
    Task<List<JsonDocument>> GetProjectsAsync(string owner, string repo, CancellationToken ct = default);

    /// <summary>
    /// Configure the API client with a base URL and authentication token.
    /// Must be called before any API requests. Can be called again to update settings.
    /// </summary>
    /// <param name="baseUrl">GitHub API base URL (e.g. https://api.github.com).</param>
    /// <param name="token">Personal Access Token.</param>
    void Configure(string baseUrl, string token);
}
