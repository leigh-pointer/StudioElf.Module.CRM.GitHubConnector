using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// A GitHub Project (classic or beta) associated with a repository.
/// </summary>
[Table("StudioElfCRMExtnGitHubProject")]
public class GitHubProject : ModelBase
{
    [Key]
    public int Id { get; set; }

    /// <summary>FK to GitHubRepository.</summary>
    public int RepositoryId { get; set; }

    /// <summary>GitHub's numeric project ID.</summary>
    public long ProjectId { get; set; }

    /// <summary>Project name.</summary>
    [Required, MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Project description/body.</summary>
    public string? Body { get; set; }

    /// <summary>State: "open", "closed".</summary>
    [MaxLength(50)]
    public string State { get; set; } = "open";

    /// <summary>HTML URL to the project.</summary>
    [MaxLength(1000)]
    public string? HtmlUrl { get; set; }

    /// <summary>Project number within the owner.</summary>
    public int? Number { get; set; }

    /// <summary>When the project was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the project was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(RepositoryId))]
    public GitHubRepository Repository { get; set; } = null!;
}
