using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// A GitHub issue or pull request synced from a tracked repository.
/// PRs are identified by <see cref="IsPullRequest"/> flag.
/// Stored in <c>StudioElfCRMExtnGitHubIssue</c>.
/// </summary>
[Table("StudioElfCRMExtnGitHubIssue")]
public class GitHubIssue : ModelBase
{
    /// <summary>Primary key.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key to <see cref="GitHubRepository"/>.</summary>
    public int RepositoryId { get; set; }

    /// <summary>GitHub issue/PR number.</summary>
    public long IssueNumber { get; set; }

    /// <summary>Issue/PR title.</summary>
    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Issue/PR body markdown.</summary>
    public string? Body { get; set; }

    /// <summary>State: "open", "closed".</summary>
    [MaxLength(50)]
    public string State { get; set; } = "open";

    /// <summary>API URL for this issue.</summary>
    [MaxLength(1000)]
    public string? Url { get; set; }

    /// <summary>HTML URL for this issue.</summary>
    [MaxLength(1000)]
    public string? HtmlUrl { get; set; }

    /// <summary>JSON array of label names.</summary>
    [MaxLength(2000)]
    public string? Labels { get; set; }

    /// <summary>GitHub login of the user who created the issue.</summary>
    [MaxLength(200)]
    public string? UserLogin { get; set; }

    /// <summary>True if this is a pull request, false if issue.</summary>
    public bool IsPullRequest { get; set; }

    /// <summary>PR merge state: "merged", "dirty", "clean", null if issue.</summary>
    [MaxLength(50)]
    public string? MergeState { get; set; }

    /// <summary>When the issue was created on GitHub.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the issue was last updated on GitHub.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>When the issue was closed, null if open.</summary>
    public DateTime? ClosedAt { get; set; }

    [ForeignKey(nameof(RepositoryId))]
    public GitHubRepository Repository { get; set; } = null!;
}
