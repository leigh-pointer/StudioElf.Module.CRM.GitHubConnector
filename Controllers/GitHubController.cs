using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Services;

namespace StudioElf.Module.GitHubConnector.Controllers;

/// <summary>
/// REST API controller for the GitHub Connector extension.
/// Provides endpoints for managing repositories, links, releases, and synchronization.
/// </summary>
[Route(ControllerRoutes.ApiRoute)]
[Route("api/crm/github")]
[Authorize]
public class GitHubController : ModuleControllerBase
{
    private readonly IGitHubRepositoryService _repositoryService;
    private readonly IGitHubReleaseService _releaseService;
    private readonly IGitHubSyncService _syncService;

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubController"/>.
    /// </summary>
    public GitHubController(
        IGitHubRepositoryService repositoryService,
        IGitHubReleaseService releaseService,
        IGitHubSyncService syncService,
        ILogManager logger,
        IHttpContextAccessor accessor)
        : base(logger, accessor)
    {
        _repositoryService = repositoryService;
        _releaseService = releaseService;
        _syncService = syncService;
    }

    // ====================================================================
    // Repositories
    // ====================================================================

    /// <summary>GET /api/crm/github/repositories?moduleId={moduleId}</summary>
    [HttpGet("repositories")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetRepositories([FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        var repos = await _repositoryService.GetAllAsync(moduleId);
        return Ok(repos);
    }

    /// <summary>GET /api/crm/github/repositories/{id}?moduleId={moduleId}</summary>
    [HttpGet("repositories/{id}")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetRepository(int id, [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            var repo = await _repositoryService.GetByIdAsync(id, moduleId);
            return Ok(repo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/crm/github/repositories?moduleId={moduleId}
    /// Add a new repository to track. Body: { "fullName": "owner/repo" }
    /// </summary>
    [HttpPost("repositories")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> AddRepository(
        [FromBody, Required] AddRepositoryDto dto,
        [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            var createdBy = User.Identity?.Name ?? "system";
            var repo = await _repositoryService.AddRepositoryAsync(dto, moduleId, createdBy);
            return Ok(repo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>DELETE /api/crm/github/repositories/{id}?moduleId={moduleId}</summary>
    [HttpDelete("repositories/{id}")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> DeleteRepository(int id, [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            await _repositoryService.DeleteAsync(id, moduleId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/crm/github/repositories/sync?moduleId={moduleId}
    /// Trigger a full synchronization of all tracked repositories and releases.
    /// </summary>
    [HttpPost("repositories/sync")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> SyncAll([FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        var result = await _syncService.SyncAllAsync(moduleId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/crm/github/repositories/{id}/sync?moduleId={moduleId}
    /// Trigger synchronization of a single repository and its releases.
    /// </summary>
    [HttpPost("repositories/{id}/sync")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> SyncRepository(int id, [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            var result = await _repositoryService.SyncRepositoryAsync(id, moduleId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ====================================================================
    // Releases
    // ====================================================================

    /// <summary>
    /// GET /api/crm/github/releases?moduleId={moduleId}&amp;repositoryId={repositoryId}
    /// Get releases for a specific repository.
    /// </summary>
    [HttpGet("releases")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetReleases(
        [FromQuery] int moduleId,
        [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            var releases = await _releaseService.GetByRepositoryAsync(repositoryId, moduleId);
            return Ok(releases);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/crm/github/releases/recent?moduleId={moduleId}&amp;count={count}
    /// Get the most recent releases across all tracked repositories.
    /// </summary>
    [HttpGet("releases/recent")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetRecentReleases(
        [FromQuery] int moduleId,
        [FromQuery] int count = 10)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        var releases = await _releaseService.GetRecentAsync(moduleId, count);
        return Ok(releases);
    }

    // ====================================================================
    // Links (repository ↔ CRM entity)
    // ====================================================================

    /// <summary>
    /// POST /api/crm/github/links?moduleId={moduleId}
    /// Link a repository to a CRM entity.
    /// </summary>
    [HttpPost("links")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> AddLink(
        [FromBody, Required] CreateGitHubRepositoryLinkDto dto,
        [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            var createdBy = User.Identity?.Name ?? "system";
            var link = await _repositoryService.AddLinkAsync(dto, moduleId, createdBy);
            return Ok(link);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>DELETE /api/crm/github/links/{id}?moduleId={moduleId}</summary>
    [HttpDelete("links/{id}")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> RemoveLink(int id, [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            await _repositoryService.RemoveLinkAsync(id, moduleId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/crm/github/links?moduleId={moduleId}&amp;repositoryId={repositoryId}
    /// Get all links for a specific repository.
    /// </summary>
    [HttpGet("links")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetLinks(
        [FromQuery] int moduleId,
        [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        var links = await _repositoryService.GetLinksAsync(repositoryId, moduleId);
        return Ok(links);
    }

    /// <summary>
    /// GET /api/crm/github/entity/{entityType}/{entityId}?moduleId={moduleId}
    /// Get all repositories linked to a specific CRM entity (Company, Contact, or Deal).
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        int entityId,
        [FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        var repos = await _repositoryService.GetByEntityAsync(entityType, entityId, moduleId);
        return Ok(repos);
    }
}
