using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Represents a GitHub repository synchronized into the CRM.
/// Maps 1:1 with a GitHub API repository object.
/// Stored in extension-specific table <c>StudioElfCRMExtnGitHubRepo</c>.
/// </summary>
[Table("StudioElfCRMExtnGitHubRepo")]
public class GitHubRepository : ModelBase
{
    /// <summary>Primary key.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Oqtane module this record belongs to.</summary>
    public int ModuleId { get; set; }

    /// <summary>GitHub's numeric repository ID (stable across renames).</summary>
    public long RepositoryId { get; set; }

    /// <summary>Repository name (e.g. "aspnetcore").</summary>
    [Required, MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Full name with owner (e.g. "dotnet/aspnetcore").</summary>
    [MaxLength(500)]
    public string? FullName { get; set; }

    /// <summary>Repository description from GitHub.</summary>
    public string? Description { get; set; }

    /// <summary>HTTPS URL to the repository on GitHub.</summary>
    [MaxLength(1000)]
    public string? Url { get; set; }

    /// <summary>Default branch name (e.g. "main", "master").</summary>
    [MaxLength(100)]
    public string? DefaultBranch { get; set; }

    /// <summary>Whether the repository is private.</summary>
    public bool IsPrivate { get; set; }

    /// <summary>Primary programming language detected by GitHub.</summary>
    [MaxLength(100)]
    public string? PrimaryLanguage { get; set; }

    /// <summary>JSON array of topic strings.</summary>
    [MaxLength(2000)]
    public string? Topics { get; set; }

    /// <summary>Star count.</summary>
    public int Stars { get; set; }

    /// <summary>Fork count.</summary>
    public int Forks { get; set; }

    /// <summary>Open issue count.</summary>
    public int OpenIssues { get; set; }

    /// <summary>Date of the most recent commit (from GitHub's pushed_at).</summary>
    public DateTime? LatestCommitAt { get; set; }

    /// <summary>When this record was last synchronized with GitHub.</summary>
    public DateTime LastSyncedOn { get; set; }
}

