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

    /// <summary>Show GitHub Overview widget on dashboard.</summary>
    public bool ShowOverviewWidget { get; set; } = true;

    /// <summary>Show Recent Releases widget on dashboard.</summary>
    public bool ShowRecentReleasesWidget { get; set; } = true;

    /// <summary>Show Analytics widget on dashboard.</summary>
    public bool ShowAnalyticsWidget { get; set; } = true;

    /// <summary>Enable issue tracking and synchronization.</summary>
    public bool EnableIssueTracking { get; set; }

    /// <summary>Enable pull request tracking and synchronization.</summary>
    public bool EnablePullRequestTracking { get; set; }

    /// <summary>Enable release tracking and synchronization.</summary>
    public bool EnableReleaseTracking { get; set; } = true;

    /// <summary>Show pre-release versions in widgets and lists.</summary>
    public bool ShowPrereleases { get; set; } = true;

    /// <summary>Max releases to show per repo in widgets.</summary>
    public int MaxReleasesPerRepo { get; set; } = 50;

    /// <summary>Release date range filter in days (0 = no filter).</summary>
    public int ReleaseDateRangeDays { get; set; }

    /// <summary>Interval in minutes between automatic background syncs. Default 30.</summary>
    public int SynchronizationIntervalMinutes { get; set; } = 30;
}
