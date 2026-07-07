using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// A GitHub Discussion synced from a repository.
/// </summary>
[Table("StudioElfCRMExtnGitHubDiscussion")]
public class GitHubDiscussion : ModelBase
{
    [Key]
    public int Id { get; set; }

    /// <summary>FK to GitHubRepository.</summary>
    public int RepositoryId { get; set; }

    /// <summary>GitHub's numeric discussion ID.</summary>
    public long DiscussionId { get; set; }

    /// <summary>Discussion title.</summary>
    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Discussion body markdown.</summary>
    public string? Body { get; set; }

    /// <summary>Discussion category (e.g. "General", "Ideas", "Q&A").</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>State: "open", "closed".</summary>
    [MaxLength(50)]
    public string State { get; set; } = "open";

    /// <summary>HTML URL to the discussion.</summary>
    [MaxLength(1000)]
    public string? HtmlUrl { get; set; }

    /// <summary>GitHub login of the author.</summary>
    [MaxLength(200)]
    public string? AuthorLogin { get; set; }

    /// <summary>When the discussion was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the discussion was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>When the discussion was answered (if applicable).</summary>
    public DateTime? AnsweredAt { get; set; }

    [ForeignKey(nameof(RepositoryId))]
    public GitHubRepository Repository { get; set; } = null!;
}
