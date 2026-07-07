using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Polymorphic link between a <see cref="GitHubRepository"/> and any CRM entity
/// (Company, Contact, or Deal). <see cref="EntityType"/> stores the CRM entity type name
/// ("company", "contact", "deal") and <see cref="EntityId"/> stores its primary key.
/// </summary>
[Table("StudioElfCRMExtnGitHubRepoLink")]
public class GitHubRepositoryLink : ModelBase
{
    /// <summary>Primary key.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key to <see cref="GitHubRepository"/>.</summary>
    public int RepositoryId { get; set; }

    /// <summary>
    /// CRM entity type name. One of: "company", "contact", "deal".
    /// Matches <c>CrmEntityNames</c> constants.
    /// </summary>
    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the linked CRM entity.</summary>
    public int EntityId { get; set; }

    /// <summary>Navigation property to the repository.</summary>
    [ForeignKey(nameof(RepositoryId))]
    public GitHubRepository Repository { get; set; } = null!;
}

