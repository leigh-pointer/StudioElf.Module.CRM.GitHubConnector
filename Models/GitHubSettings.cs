namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Extension settings stored as serialized JSON in CRM module settings.
/// Read/written by the shell component via <c>ISettingService</c> with key <c>"GitHubConnectorSettings"</c>.
/// </summary>
/// <remarks>
/// Phase 1 uses Personal Access Token authentication.
/// Phase 2 will add GitHub App OAuth support.
/// Phase 3 will add GitHub Enterprise Server support (custom base URL).
/// </remarks>
public class GitHubSettings
{
    /// <summary>Settings storage key in the CRM module settings system.</summary>
    public const string SettingsKey = "GitHubConnectorSettings";

    /// <summary>
    /// GitHub API base URL. Defaults to https://api.github.com for public GitHub.
    /// For GitHub Enterprise Server, change to <c>https://[hostname]/api/v3</c>.
    /// </summary>
    public string GitHubApiUrl { get; set; } = "https://api.github.com";

    /// <summary>
    /// Personal Access Token for API authentication (Phase 1).
    /// Requires repo scope for private repositories.
    /// </summary>
    public string? PersonalAccessToken { get; set; }

    /// <summary>Enable incoming webhook processing. (Phase 3)</summary>
    public bool EnableWebhooks { get; set; }

    /// <summary>Enable issue tracking and synchronization. (Phase 2)</summary>
    public bool EnableIssueTracking { get; set; }

    /// <summary>Enable pull request tracking and synchronization. (Phase 2)</summary>
    public bool EnablePullRequestTracking { get; set; }

    /// <summary>Enable release tracking and synchronization.</summary>
    public bool EnableReleaseTracking { get; set; } = true;

    /// <summary>Interval in minutes between automatic background syncs. Default 30.</summary>
    public int SynchronizationIntervalMinutes { get; set; } = 30;
}
