using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Service for CRUD operations on tracked GitHub repositories
/// and their polymorphic links to CRM entities.
/// </summary>
public interface IGitHubRepositoryService
{
    /// <summary>Get all tracked repositories for a module.</summary>
    Task<List<GitHubRepositoryDto>> GetAllAsync(int moduleId);

    /// <summary>Get a single repository by its primary key.</summary>
    Task<GitHubRepositoryDto> GetByIdAsync(int id, int moduleId);

    /// <summary>Get a repository by its GitHub-side ID (stable across renames).</summary>
    Task<GitHubRepositoryDto> GetByRepositoryIdAsync(long repositoryId, int moduleId);

    /// <summary>Add a new repository to track by its full name (e.g. "owner/repo").</summary>
    Task<GitHubRepositoryDto> AddRepositoryAsync(AddRepositoryDto dto, int moduleId, string createdBy);

    /// <summary>Remove a tracked repository and its links/releases.</summary>
    Task DeleteAsync(int id, int moduleId);

    // Link management

    /// <summary>Link a repository to a CRM entity.</summary>
    Task<GitHubRepositoryLinkDto> AddLinkAsync(CreateGitHubRepositoryLinkDto dto, int moduleId, string createdBy);

    /// <summary>Remove a link between a repository and a CRM entity.</summary>
    Task RemoveLinkAsync(int linkId, int moduleId);

    /// <summary>Get all repositories linked to a specific CRM entity.</summary>
    Task<List<GitHubRepositoryDto>> GetByEntityAsync(string entityType, int entityId, int moduleId);

    /// <summary>Get all links for a specific repository.</summary>
    Task<List<GitHubRepositoryLinkDto>> GetLinksAsync(int repositoryId, int moduleId);

    // Synchronization

    /// <summary>Sync all tracked repositories with GitHub API.</summary>
    Task<SyncResultDto> SyncAllAsync(int moduleId, CancellationToken ct = default);

    /// <summary>Sync a single repository with GitHub API.</summary>
    Task<SyncResultDto> SyncRepositoryAsync(int repositoryId, int moduleId, CancellationToken ct = default);
}
