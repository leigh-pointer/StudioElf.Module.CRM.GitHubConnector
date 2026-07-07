namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Data transfer object for <see cref="GitHubIssue"/>.
/// </summary>
public class GitHubIssueDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public long IssueNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string State { get; set; } = "open";
    public string? Url { get; set; }
    public string? HtmlUrl { get; set; }
    public string? Labels { get; set; }
    public string? UserLogin { get; set; }
    public bool IsPullRequest { get; set; }
    public string? MergeState { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
