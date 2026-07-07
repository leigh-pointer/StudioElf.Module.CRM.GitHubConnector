using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubActionService : IGitHubActionService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _factory;
    private readonly IGitHubApiClient _api;
    private readonly ILogger<GitHubActionService> _logger;

    public GitHubActionService(IDbContextFactory<GitHubConnectorContext> factory, IGitHubApiClient api, ILogger<GitHubActionService> logger)
    { _factory = factory; _api = api; _logger = logger; }

    public async Task<List<GitHubActionWorkflowDto>> GetByRepositoryAsync(int repositoryId, int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GitHubActionWorkflows.Where(w => w.RepositoryId == repositoryId && w.Repository.ModuleId == moduleId)
            .OrderByDescending(w => w.CreatedAt).Select(w => ToDto(w)).ToListAsync();
    }

    public async Task<int> SyncActionsAsync(int repositoryId, int moduleId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var repo = await db.GitHubRepositories.FirstOrDefaultAsync(r => r.Id == repositoryId && r.ModuleId == moduleId, ct);
        if (repo == null) throw new KeyNotFoundException();

        var parts = repo.FullName!.Split('/');
        var runs = await _api.GetActionsAsync(parts[0], parts[1], ct);
        var count = 0;

        foreach (var json in runs)
        {
            ct.ThrowIfCancellationRequested();
            var r = json.RootElement;
            var runId = r.GetProperty("id").GetInt64();

            var existing = await db.GitHubActionWorkflows.FirstOrDefaultAsync(w => w.RunId == runId && w.RepositoryId == repositoryId, ct);
            if (existing == null)
            {
                existing = new GitHubActionWorkflow { RepositoryId = repositoryId, RunId = runId, CreatedOn = DateTime.UtcNow };
                db.GitHubActionWorkflows.Add(existing);
            }

            existing.WorkflowName = r.TryGetProperty("name", out var n) ? n.GetString() : null;
            existing.Branch = r.TryGetProperty("head_branch", out var b) ? b.GetString() : null;
            existing.HeadSha = r.TryGetProperty("head_sha", out var s) ? s.GetString() : null;
            existing.Status = r.GetProperty("status").GetString() ?? "unknown";
            existing.Conclusion = r.TryGetProperty("conclusion", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() : null;
            existing.HtmlUrl = r.TryGetProperty("html_url", out var u) ? u.GetString() : null;
            existing.RunNumber = r.TryGetProperty("run_number", out var num) ? num.GetInt32() : null;
            existing.TriggerEvent = r.TryGetProperty("event", out var ev) ? ev.GetString() : null;
            existing.CreatedAt = DateTime.Parse(r.GetProperty("created_at").GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            existing.UpdatedAt = r.TryGetProperty("updated_at", out var up) && up.ValueKind == JsonValueKind.String
                ? DateTime.Parse(up.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal) : null;
            existing.ModifiedOn = DateTime.UtcNow;
            count++;
        }
        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Synced {Count} workflow runs for repo {RepoId}", count, repositoryId);
        return count;
    }

    private static GitHubActionWorkflowDto ToDto(GitHubActionWorkflow e) => new()
    {
        Id = e.Id, RepositoryId = e.RepositoryId, RepositoryName = e.Repository?.FullName ?? "",
        RunId = e.RunId, WorkflowName = e.WorkflowName, Branch = e.Branch,
        Status = e.Status, Conclusion = e.Conclusion, HtmlUrl = e.HtmlUrl,
        RunNumber = e.RunNumber, TriggerEvent = e.TriggerEvent, CreatedAt = e.CreatedAt
    };
}
