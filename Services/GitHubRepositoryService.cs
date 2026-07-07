using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Default implementation of <see cref="IGitHubRepositoryService"/>.
/// Manages repository CRUD, entity linking, and GitHub API synchronization.
/// </summary>
public class GitHubRepositoryService : IGitHubRepositoryService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _contextFactory;
    private readonly IGitHubApiClient _apiClient;
    private readonly ILogger<GitHubRepositoryService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubRepositoryService"/>.
    /// </summary>
    public GitHubRepositoryService(
        IDbContextFactory<GitHubConnectorContext> contextFactory,
        IGitHubApiClient apiClient,
        ILogger<GitHubRepositoryService> logger)
    {
        _contextFactory = contextFactory;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<GitHubRepositoryDto>> GetAllAsync(int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.GitHubRepositories
            .Where(r => r.ModuleId == moduleId)
            .OrderByDescending(r => r.LastSyncedOn)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<GitHubRepositoryDto> GetByIdAsync(int id, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.Id == id && r.ModuleId == moduleId);

        if (entity == null)
            throw new KeyNotFoundException($"GitHubRepository {id} not found in module {moduleId}.");

        var dto = ToDto(entity);
        dto.LinkedEntities = await ResolveLinkedEntityNamesAsync(db, id);
        return dto;
    }

    public async Task<GitHubRepositoryDto> GetByRepositoryIdAsync(long repositoryId, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.RepositoryId == repositoryId && r.ModuleId == moduleId);

        if (entity == null)
            throw new KeyNotFoundException(
                $"GitHubRepository with RepositoryId {repositoryId} not found in module {moduleId}.");

        return ToDto(entity);
    }

    public async Task<GitHubRepositoryDto> AddRepositoryAsync(AddRepositoryDto dto, int moduleId, string createdBy)
    {
        // Parse "owner/repo" format
        var parts = dto.FullName.Split('/');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new ArgumentException(
                "Repository full name must be in the format 'owner/repo' (e.g. 'dotnet/aspnetcore').",
                nameof(dto));

        var owner = parts[0].Trim();
        var repo = parts[1].Trim();

        // Fetch from GitHub API to validate and get metadata
        var json = await _apiClient.GetRepositoryAsync(owner, repo);
        var root = json.RootElement;

        var entity = new GitHubRepository
        {
            ModuleId = moduleId,
            RepositoryId = root.GetProperty("id").GetInt64(),
            Name = root.GetProperty("name").GetString() ?? repo,
            FullName = root.GetProperty("full_name").GetString() ?? dto.FullName,
            Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            Url = root.TryGetProperty("html_url", out var htmlUrl) ? htmlUrl.GetString() : null,
            DefaultBranch = root.TryGetProperty("default_branch", out var branch) ? branch.GetString() : null,
            IsPrivate = root.TryGetProperty("private", out var isPrivate) && isPrivate.GetBoolean(),
            PrimaryLanguage = root.TryGetProperty("language", out var lang) ? lang.GetString() : null,
            Stars = root.TryGetProperty("stargazers_count", out var stars) ? stars.GetInt32() : 0,
            Forks = root.TryGetProperty("forks_count", out var forks) ? forks.GetInt32() : 0,
            OpenIssues = root.TryGetProperty("open_issues_count", out var issues) ? issues.GetInt32() : 0,
            LatestCommitAt = root.TryGetProperty("pushed_at", out var pushed) && pushed.ValueKind == JsonValueKind.String
                ? DateTime.Parse(pushed.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                : null,
            LastSyncedOn = DateTime.UtcNow,
        };
        // Set ModelBase audit fields
        entity.CreatedBy = createdBy;
        entity.CreatedOn = DateTime.UtcNow;
        entity.ModifiedBy = createdBy;
        entity.ModifiedOn = DateTime.UtcNow;

        await using var db = await _contextFactory.CreateDbContextAsync();

        // Check for duplicate by RepositoryId
        var existing = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.RepositoryId == entity.RepositoryId && r.ModuleId == moduleId);

        if (existing != null)
            throw new InvalidOperationException(
                $"Repository '{dto.FullName}' is already being tracked (Id: {existing.Id}).");

        db.GitHubRepositories.Add(entity);
        await db.SaveChangesAsync();

        _logger.LogInformation("Added GitHub repository {FullName} (RepoId: {RepoId}) to module {ModuleId}",
            entity.FullName, entity.RepositoryId, moduleId);

        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.Id == id && r.ModuleId == moduleId);

        if (entity == null)
            throw new KeyNotFoundException($"GitHubRepository {id} not found in module {moduleId}.");

        db.GitHubRepositories.Remove(entity);
        await db.SaveChangesAsync();

        _logger.LogInformation("Removed GitHub repository {FullName} (Id: {Id}) from module {ModuleId}",
            entity.FullName, id, moduleId);
    }

    // --- Link management ---

    public async Task<GitHubRepositoryLinkDto> AddLinkAsync(CreateGitHubRepositoryLinkDto dto, int moduleId, string createdBy)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        // Verify repository exists
        var repo = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.Id == dto.RepositoryId && r.ModuleId == moduleId);
        if (repo == null)
            throw new KeyNotFoundException($"Repository {dto.RepositoryId} not found in module {moduleId}.");

        // Check for duplicate link
        var existing = await db.GitHubRepositoryLinks
            .FirstOrDefaultAsync(l =>
                l.RepositoryId == dto.RepositoryId &&
                l.EntityType == dto.EntityType &&
                l.EntityId == dto.EntityId);
        if (existing != null)
            throw new InvalidOperationException("This repository is already linked to the specified entity.");

        var link = new GitHubRepositoryLink
        {
            RepositoryId = dto.RepositoryId,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
        };
        link.CreatedBy = createdBy;
        link.CreatedOn = DateTime.UtcNow;
        link.ModifiedBy = createdBy;
        link.ModifiedOn = DateTime.UtcNow;

        db.GitHubRepositoryLinks.Add(link);
        await db.SaveChangesAsync();

        _logger.LogInformation("Linked repository {RepoId} to {EntityType}:{EntityId}",
            dto.RepositoryId, dto.EntityType, dto.EntityId);

        return ToLinkDto(link);
    }

    public async Task RemoveLinkAsync(int linkId, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var link = await db.GitHubRepositoryLinks
            .Include(l => l.Repository)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Repository.ModuleId == moduleId);

        if (link == null)
            throw new KeyNotFoundException($"GitHubRepositoryLink {linkId} not found in module {moduleId}.");

        db.GitHubRepositoryLinks.Remove(link);
        await db.SaveChangesAsync();

        _logger.LogInformation("Removed link {LinkId} (Repo: {RepoId} → {EntityType}:{EntityId})",
            linkId, link.RepositoryId, link.EntityType, link.EntityId);
    }

    public async Task<List<GitHubRepositoryDto>> GetByEntityAsync(string entityType, int entityId, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var repos = await db.GitHubRepositoryLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && l.Repository.ModuleId == moduleId)
            .Select(l => l.Repository)
            .Select(r => ToDto(r))
            .ToListAsync();

        return repos;
    }

    public async Task<List<GitHubRepositoryLinkDto>> GetLinksAsync(int repositoryId, int moduleId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.GitHubRepositoryLinks
            .Where(l => l.RepositoryId == repositoryId && l.Repository.ModuleId == moduleId)
            .Select(l => ToLinkDto(l))
            .ToListAsync();
    }

    // --- Synchronization ---

    public async Task<SyncResultDto> SyncAllAsync(int moduleId, CancellationToken ct = default)
    {
        var result = new SyncResultDto();

        await using var db = await _contextFactory.CreateDbContextAsync();
        var repos = await db.GitHubRepositories
            .Where(r => r.ModuleId == moduleId)
            .ToListAsync(ct);

        if (repos.Count == 0)
        {
            result.Message = "No repositories to sync. Add a repository first.";
            result.Success = true;
            return result;
        }

        foreach (var repo in repos)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var parts = repo.FullName?.Split('/');
                if (parts == null || parts.Length != 2)
                    continue;

                var json = await _apiClient.GetRepositoryAsync(parts[0], parts[1], ct);
                var root = json.RootElement;

                repo.Name = root.GetProperty("name").GetString() ?? repo.Name;
                repo.FullName = root.GetProperty("full_name").GetString() ?? repo.FullName;
                repo.Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : repo.Description;
                repo.Url = root.TryGetProperty("html_url", out var htmlUrl) ? htmlUrl.GetString() : repo.Url;
                repo.DefaultBranch = root.TryGetProperty("default_branch", out var branch) ? branch.GetString() : repo.DefaultBranch;
                repo.IsPrivate = root.TryGetProperty("private", out var isPrivate) && isPrivate.GetBoolean();
                repo.PrimaryLanguage = root.TryGetProperty("language", out var lang) ? lang.GetString() : repo.PrimaryLanguage;
                repo.Stars = root.TryGetProperty("stargazers_count", out var stars) ? stars.GetInt32() : repo.Stars;
                repo.Forks = root.TryGetProperty("forks_count", out var forks) ? forks.GetInt32() : repo.Forks;
                repo.OpenIssues = root.TryGetProperty("open_issues_count", out var issues) ? issues.GetInt32() : repo.OpenIssues;
                repo.LatestCommitAt = root.TryGetProperty("pushed_at", out var pushed) && pushed.ValueKind == JsonValueKind.String
                    ? DateTime.Parse(pushed.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                    : repo.LatestCommitAt;
                repo.LastSyncedOn = DateTime.UtcNow;
                repo.ModifiedOn = DateTime.UtcNow;

                result.RepositoriesUpdated++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync repository {FullName}", repo.FullName);
                result.Errors.Add($"Failed to sync '{repo.FullName}': {ex.Message}");
                result.Success = false;
            }
        }

        await db.SaveChangesAsync(ct);

        result.Message = $"Synced {result.RepositoriesUpdated} repositories. {result.Errors.Count} error(s).";
        return result;
    }

    public async Task<SyncResultDto> SyncRepositoryAsync(int repositoryId, int moduleId, CancellationToken ct = default)
    {
        var result = new SyncResultDto();

        await using var db = await _contextFactory.CreateDbContextAsync();
        var repo = await db.GitHubRepositories
            .FirstOrDefaultAsync(r => r.Id == repositoryId && r.ModuleId == moduleId, ct);

        if (repo == null)
            throw new KeyNotFoundException($"Repository {repositoryId} not found in module {moduleId}.");

        try
        {
            var parts = repo.FullName?.Split('/');
            if (parts == null || parts.Length != 2)
                throw new InvalidOperationException($"Repository '{repo.FullName}' has an invalid full name format.");

            var json = await _apiClient.GetRepositoryAsync(parts[0], parts[1], ct);
            var root = json.RootElement;

            repo.Name = root.GetProperty("name").GetString() ?? repo.Name;
            repo.Stars = root.TryGetProperty("stargazers_count", out var stars) ? stars.GetInt32() : repo.Stars;
            repo.Forks = root.TryGetProperty("forks_count", out var forks) ? forks.GetInt32() : repo.Forks;
            repo.OpenIssues = root.TryGetProperty("open_issues_count", out var issues) ? issues.GetInt32() : repo.OpenIssues;
            repo.LastSyncedOn = DateTime.UtcNow;
            repo.ModifiedOn = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            result.RepositoriesUpdated = 1;
            result.Message = $"Repository '{repo.FullName}' synced successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync repository {FullName}", repo.FullName);
            result.Success = false;
            result.Errors.Add(ex.Message);
            result.Message = $"Sync failed: {ex.Message}";
        }

        return result;
    }

    // --- Mapping helpers ---

    private static GitHubRepositoryDto ToDto(GitHubRepository entity)
    {
        return new GitHubRepositoryDto
        {
            Id = entity.Id,
            ModuleId = entity.ModuleId,
            RepositoryId = entity.RepositoryId,
            Name = entity.Name,
            FullName = entity.FullName,
            Description = entity.Description,
            Url = entity.Url,
            DefaultBranch = entity.DefaultBranch,
            IsPrivate = entity.IsPrivate,
            PrimaryLanguage = entity.PrimaryLanguage,
            Topics = entity.Topics,
            Stars = entity.Stars,
            Forks = entity.Forks,
            OpenIssues = entity.OpenIssues,
            LatestCommitAt = entity.LatestCommitAt,
            LastSyncedOn = entity.LastSyncedOn,
        };
    }

    private static GitHubRepositoryLinkDto ToLinkDto(GitHubRepositoryLink link)
    {
        return new GitHubRepositoryLinkDto
        {
            Id = link.Id,
            RepositoryId = link.RepositoryId,
            EntityType = link.EntityType,
            EntityId = link.EntityId,
        };
    }

    private static async Task<List<string>> ResolveLinkedEntityNamesAsync(GitHubConnectorContext db, int repositoryId)
    {
        var links = await db.GitHubRepositoryLinks
            .Where(l => l.RepositoryId == repositoryId)
            .Select(l => $"{l.EntityType}:{l.EntityId}")
            .ToListAsync();
        return links;
    }
}

