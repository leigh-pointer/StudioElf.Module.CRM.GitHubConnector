using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubProjectService : IGitHubProjectService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _factory;
    private readonly IGitHubApiClient _api;
    private readonly ILogger<GitHubProjectService> _logger;

    public GitHubProjectService(IDbContextFactory<GitHubConnectorContext> factory, IGitHubApiClient api, ILogger<GitHubProjectService> logger)
    { _factory = factory; _api = api; _logger = logger; }

    public async Task<List<GitHubProjectDto>> GetByRepositoryAsync(int repositoryId, int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GitHubProjects.Where(p => p.RepositoryId == repositoryId && p.Repository.ModuleId == moduleId)
            .OrderByDescending(p => p.CreatedAt).Select(p => new GitHubProjectDto
            { Id = p.Id, RepositoryId = p.RepositoryId, RepositoryName = p.Repository.FullName ?? "", ProjectId = p.ProjectId,
              Name = p.Name, Body = p.Body, State = p.State, HtmlUrl = p.HtmlUrl, Number = p.Number, CreatedAt = p.CreatedAt }).ToListAsync();
    }

    public async Task<int> SyncProjectsAsync(int repositoryId, int moduleId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var repo = await db.GitHubRepositories.FirstOrDefaultAsync(r => r.Id == repositoryId && r.ModuleId == moduleId, ct);
        if (repo == null) throw new KeyNotFoundException();
        var parts = repo.FullName!.Split('/');
        var items = await _api.GetProjectsAsync(parts[0], parts[1], ct);
        var count = 0;

        foreach (var json in items)
        {
            ct.ThrowIfCancellationRequested();
            var r = json.RootElement;
            var projectId = r.GetProperty("id").GetInt64();
            var existing = await db.GitHubProjects.FirstOrDefaultAsync(p => p.ProjectId == projectId && p.RepositoryId == repositoryId, ct);
            if (existing == null)
            {
                existing = new GitHubProject { RepositoryId = repositoryId, ProjectId = projectId, CreatedOn = DateTime.UtcNow };
                db.GitHubProjects.Add(existing);
            }
            existing.Name = r.GetProperty("name").GetString() ?? "";
            existing.Body = r.TryGetProperty("body", out var body) ? body.GetString() : null;
            existing.State = r.GetProperty("state").GetString() ?? "open";
            existing.HtmlUrl = r.TryGetProperty("html_url", out var url) ? url.GetString() : null;
            existing.Number = r.TryGetProperty("number", out var num) ? num.GetInt32() : null;
            existing.CreatedAt = DateTime.Parse(r.GetProperty("created_at").GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            existing.UpdatedAt = r.TryGetProperty("updated_at", out var up) && up.ValueKind == JsonValueKind.String
                ? DateTime.Parse(up.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal) : null;
            existing.ModifiedOn = DateTime.UtcNow;
            count++;
        }
        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Synced {Count} projects for repo {RepoId}", count, repositoryId);
        return count;
    }
}
