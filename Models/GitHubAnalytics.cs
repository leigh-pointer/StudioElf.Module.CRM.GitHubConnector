namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>Aggregated analytics for tracked repositories.</summary>
public class GitHubAnalyticsDto
{
    public int TotalRepos { get; set; }
    public int TotalStars { get; set; }
    public int TotalForks { get; set; }
    public int OpenIssues { get; set; }
    public int TotalReleases { get; set; }
    public int TotalWorkflowRuns { get; set; }
    public int SuccessfulRuns { get; set; }
    public int FailedRuns { get; set; }

    /// <summary>Releases per month for the last 6 months.</summary>
    public List<MonthlyCount> ReleasesPerMonth { get; set; } = new();

    /// <summary>Most starred repos.</summary>
    public List<RepoSummary> TopRepos { get; set; } = new();
}

public class MonthlyCount
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}

public class RepoSummary
{
    public string Name { get; set; } = "";
    public int Stars { get; set; }
    public int OpenIssues { get; set; }
    public DateTime? LatestRelease { get; set; }
}
