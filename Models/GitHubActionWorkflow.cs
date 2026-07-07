using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// A GitHub Actions workflow run synced from a repository.
/// </summary>
[Table("StudioElfCRMExtnGitHubActionWorkflow")]
public class GitHubActionWorkflow : ModelBase
{
    [Key]
    public int Id { get; set; }

    /// <summary>FK to GitHubRepository.</summary>
    public int RepositoryId { get; set; }

    /// <summary>GitHub's numeric workflow run ID.</summary>
    public long RunId { get; set; }

    /// <summary>Workflow name (e.g. "CI", "Deploy").</summary>
    [MaxLength(500)]
    public string? WorkflowName { get; set; }

    /// <summary>Branch the workflow ran on.</summary>
    [MaxLength(500)]
    public string? Branch { get; set; }

    /// <summary>Head branch for PR-triggered runs.</summary>
    [MaxLength(500)]
    public string? HeadBranch { get; set; }

    /// <summary>Commit SHA.</summary>
    [MaxLength(100)]
    public string? HeadSha { get; set; }

    /// <summary>Run status: "queued", "in_progress", "completed".</summary>
    [MaxLength(50)]
    public string Status { get; set; } = "queued";

    /// <summary>Run conclusion: "success", "failure", "cancelled", null if in progress.</summary>
    [MaxLength(50)]
    public string? Conclusion { get; set; }

    /// <summary>HTML URL to the workflow run.</summary>
    [MaxLength(1000)]
    public string? HtmlUrl { get; set; }

    /// <summary>Run number.</summary>
    public int? RunNumber { get; set; }

    /// <summary>Event that triggered the run (e.g. "push", "pull_request").</summary>
    [MaxLength(100)]
    public string? TriggerEvent { get; set; }

    /// <summary>When the run was created on GitHub.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the run was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(RepositoryId))]
    public GitHubRepository Repository { get; set; } = null!;
}
