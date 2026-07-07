using System.ComponentModel.DataAnnotations;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Data transfer object for <see cref="GitHubRepository"/>.
/// Includes linked entity names for display.
/// </summary>
public class GitHubRepositoryDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public long RepositoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? DefaultBranch { get; set; }
    public bool IsPrivate { get; set; }
    public string? PrimaryLanguage { get; set; }
    public string? Topics { get; set; }
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public DateTime? LatestCommitAt { get; set; }
    public DateTime LastSyncedOn { get; set; }

    /// <summary>Human-readable descriptions of linked CRM entities, e.g., "Company: Acme Corp".</summary>
    public List<string> LinkedEntities { get; set; } = new();
}

/// <summary>
/// DTO for <see cref="GitHubRepositoryLink"/>.
/// <see cref="EntityName"/> is resolved from CRM at query time.
/// </summary>
public class GitHubRepositoryLinkDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }

    /// <summary>Resolved display name of the linked CRM entity.</summary>
    public string? EntityName { get; set; }
}

/// <summary>Request payload to create a new repository link.</summary>
public class CreateGitHubRepositoryLinkDto
{
    /// <summary>FK to the repository to link.</summary>
    public int RepositoryId { get; set; }

    /// <summary>CRM entity type: "company", "contact", or "deal".</summary>
    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the CRM entity.</summary>
    public int EntityId { get; set; }
}

/// <summary>Request payload to add a repository for tracking by its full name.</summary>
public class AddRepositoryDto
{
    /// <summary>Full repository name including owner, e.g. "dotnet/aspnetcore".</summary>
    [Required, MaxLength(500)]
    public string FullName { get; set; } = string.Empty;
}

/// <summary>Result of a synchronization operation, whether manual or background.</summary>
public class SyncResultDto
{
    /// <summary>True if all sync operations completed without error.</summary>
    public bool Success { get; set; } = true;

    /// <summary>Human-readable summary message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Number of repositories updated during this sync.</summary>
    public int RepositoriesUpdated { get; set; }

    /// <summary>Number of releases updated during this sync.</summary>
    public int ReleasesUpdated { get; set; }

    /// <summary>List of error messages, if any.</summary>
    public List<string> Errors { get; set; } = new();
}

