using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// A GitHub release associated with a tracked repository.
/// Synced from GitHub Releases API.
/// </summary>
[Table("StudioElfCRMExtnGitHubRelease")]
public class GitHubRelease : ModelBase
{
    /// <summary>Primary key.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key to <see cref="GitHubRepository"/>.</summary>
    public int RepositoryId { get; set; }

    /// <summary>GitHub's numeric release ID (stable across edits).</summary>
    public long ReleaseId { get; set; }

    /// <summary>Git tag name (e.g. "v1.0.0").</summary>
    [MaxLength(100)]
    public string? TagName { get; set; }

    /// <summary>Release title set by the author.</summary>
    [MaxLength(500)]
    public string? ReleaseName { get; set; }

    /// <summary>Release body / changelog markdown.</summary>
    public string? Body { get; set; }

    /// <summary>HTTPS URL to the release on GitHub.</summary>
    [MaxLength(1000)]
    public string? Url { get; set; }

    /// <summary>Whether this is a pre-release (not ready for production).</summary>
    public bool IsPrerelease { get; set; }

    /// <summary>When the release was published on GitHub.</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>Navigation property to the repository.</summary>
    [ForeignKey(nameof(RepositoryId))]
    public GitHubRepository Repository { get; set; } = null!;
}

