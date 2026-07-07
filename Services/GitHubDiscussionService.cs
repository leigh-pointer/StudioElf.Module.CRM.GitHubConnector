using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubDiscussionService : IGitHubDiscussionService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _factory;
    private readonly IGitHubApiClient _api;
    private readonly ILogger<GitHubDiscussionService> _logger;

    public GitHubDiscussionService(IDbContextFactory<GitHubConnectorContext> factory, IGitHubApiClient api, ILogger<GitHubDiscussionService> logger)
    { _factory = factory; _api = api; _logger = logger; }

    public async Task<List<GitHubDiscussionDto>> GetByRepositoryAsync(int repositoryId, int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GitHubDiscussions.Where(d => d.RepositoryId == repositoryId && d.Repository.ModuleId == moduleId)
            .OrderByDescending(d => d.CreatedAt).Select(d => new GitHubDiscussionDto
            { Id = d.Id, RepositoryId = d.RepositoryId, RepositoryName = d.Repository.FullName ?? "", DiscussionId = d.DiscussionId,
              Title = d.Title, Body = d.Body, Category = d.Category, State = d.State, HtmlUrl = d.HtmlUrl,
              AuthorLogin = d.AuthorLogin, CreatedAt = d.CreatedAt }).ToListAsync();
    }

    public async Task<int> SyncDiscussionsAsync(int repositoryId, int moduleId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var repo = await db.GitHubRepositories.FirstOrDefaultAsync(r => r.Id == repositoryId && r.ModuleId == moduleId, ct);
        if (repo == null) throw new KeyNotFoundException();
        var parts = repo.FullName!.Split('/');
        var items = await _api.GetDiscussionsAsync(parts[0], parts[1], ct);
        var count = 0;

        foreach (var json in items)
        {
            ct.ThrowIfCancellationRequested();
            var r = json.RootElement;
            var discussionId = r.GetProperty("id").GetInt64();
            var existing = await db.GitHubDiscussions.FirstOrDefaultAsync(d => d.DiscussionId == discussionId && d.RepositoryId == repositoryId, ct);
            if (existing == null)
            {
                existing = new GitHubDiscussion { RepositoryId = repositoryId, DiscussionId = discussionId, CreatedOn = DateTime.UtcNow };
                db.GitHubDiscussions.Add(existing);
            }
            existing.Title = r.GetProperty("title").GetString() ?? "";
            existing.Body = r.TryGetProperty("body", out var body) ? body.GetString() : null;
            existing.Category = r.TryGetProperty("category", out var cat) ? cat.GetProperty("name").GetString() : null;
            existing.State = r.GetProperty("state").GetString() ?? "open";
            existing.HtmlUrl = r.TryGetProperty("html_url", out var url) ? url.GetString() : null;
            existing.AuthorLogin = r.TryGetProperty("user", out var user) ? user.GetProperty("login").GetString() : null;
            existing.CreatedAt = DateTime.Parse(r.GetProperty("created_at").GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            existing.UpdatedAt = r.TryGetProperty("updated_at", out var up) && up.ValueKind == JsonValueKind.String
                ? DateTime.Parse(up.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal) : null;
            existing.ModifiedOn = DateTime.UtcNow;
            count++;
        }
        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Synced {Count} discussions for repo {RepoId}", count, repositoryId);
        return count;
    }
}
