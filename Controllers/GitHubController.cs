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
    private readonly IGitHubIssueService _issueService;
    private readonly IGitHubWebhookService _webhookService;
    private readonly IGitHubActionService _actionService;
    private readonly IGitHubDiscussionService _discussionService;
    private readonly IGitHubProjectService _projectService;
    private readonly IGitHubAnalyticsService _analyticsService;
    private readonly IGitHubSyncService _syncService;

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubController"/>.
    /// </summary>
    public GitHubController(
        IGitHubRepositoryService repositoryService,
        IGitHubReleaseService releaseService,
        IGitHubIssueService issueService,
        IGitHubWebhookService webhookService,
        IGitHubActionService actionService,
        IGitHubDiscussionService discussionService,
        IGitHubProjectService projectService,
        IGitHubAnalyticsService analyticsService,
        IGitHubSyncService syncService,
        ILogManager logger,
        IHttpContextAccessor accessor)
        : base(logger, accessor)
    {
        _repositoryService = repositoryService;
        _releaseService = releaseService;
        _issueService = issueService;
        _webhookService = webhookService;
        _actionService = actionService;
        _discussionService = discussionService;
        _projectService = projectService;
        _analyticsService = analyticsService;
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
    // Issues & Pull Requests
    // ====================================================================

    /// <summary>GET /api/crm/github/issues?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpGet("issues")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetIssues(
        [FromQuery] int moduleId,
        [FromQuery] int repositoryId,
        [FromQuery] string? state = null)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        try
        {
            var issues = await _issueService.GetByRepositoryAsync(repositoryId, moduleId, state);
            return Ok(issues);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/crm/github/issues/entity?moduleId={moduleId}&amp;entityType={entityType}&amp;entityId={entityId}
    /// Get open issues linked to a CRM entity via its repositories.
    /// </summary>
    [HttpGet("issues/entity")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetIssuesByEntity(
        [FromQuery] int moduleId,
        [FromQuery] string entityType,
        [FromQuery] int entityId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId))
            return Forbid();

        var issues = await _issueService.GetByEntityAsync(entityType, entityId, moduleId);
        return Ok(issues);
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

    // ====================================================================
    // Actions
    // ====================================================================

    /// <summary>GET /api/crm/github/actions?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpGet("actions")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetActions([FromQuery] int moduleId, [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        return Ok(await _actionService.GetByRepositoryAsync(repositoryId, moduleId));
    }

    /// <summary>POST /api/crm/github/actions/sync?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpPost("actions/sync")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> SyncActions([FromQuery] int moduleId, [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        var count = await _actionService.SyncActionsAsync(repositoryId, moduleId);
        return Ok(new { synced = count });
    }

    // ====================================================================
    // Discussions
    // ====================================================================

    /// <summary>GET /api/crm/github/discussions?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpGet("discussions")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetDiscussions([FromQuery] int moduleId, [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        return Ok(await _discussionService.GetByRepositoryAsync(repositoryId, moduleId));
    }

    /// <summary>POST /api/crm/github/discussions/sync?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpPost("discussions/sync")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> SyncDiscussions([FromQuery] int moduleId, [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        var count = await _discussionService.SyncDiscussionsAsync(repositoryId, moduleId);
        return Ok(new { synced = count });
    }

    // ====================================================================
    // Projects
    // ====================================================================

    /// <summary>GET /api/crm/github/projects?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpGet("projects")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetProjects([FromQuery] int moduleId, [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        return Ok(await _projectService.GetByRepositoryAsync(repositoryId, moduleId));
    }

    /// <summary>POST /api/crm/github/projects/sync?moduleId={moduleId}&amp;repositoryId={repositoryId}</summary>
    [HttpPost("projects/sync")]
    [Authorize(Policy = PolicyNames.EditModule)]
    public async Task<IActionResult> SyncProjects([FromQuery] int moduleId, [FromQuery] int repositoryId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        var count = await _projectService.SyncProjectsAsync(repositoryId, moduleId);
        return Ok(new { synced = count });
    }

    // ====================================================================
    // Analytics
    // ====================================================================

    /// <summary>GET /api/crm/github/analytics?moduleId={moduleId}</summary>
    [HttpGet("analytics")]
    [Authorize(Policy = PolicyNames.ViewModule)]
    public async Task<IActionResult> GetAnalytics([FromQuery] int moduleId)
    {
        if (!IsAuthorizedEntityId(EntityNames.Module, moduleId)) return Forbid();
        return Ok(await _analyticsService.GetAnalyticsAsync(moduleId));
    }

    // ====================================================================
    // Webhooks
    // ====================================================================

    /// <summary>
    /// POST /api/crm/github/webhook?moduleId={moduleId}
    /// Receives GitHub webhook events. Public endpoint, no auth.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ReceiveWebhook([FromQuery] int moduleId)
    {
        var eventType = Request.Headers["X-GitHub-Event"].FirstOrDefault() ?? "unknown";
        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();

        string payload;
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync();
        }

        var result = await _webhookService.ProcessAsync(eventType, payload, signature, moduleId);

        if (result.Success)
            return Ok(result);

        return StatusCode(500, result);
    }
}
