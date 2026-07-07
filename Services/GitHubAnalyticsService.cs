using Microsoft.EntityFrameworkCore;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubAnalyticsService : IGitHubAnalyticsService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _factory;

    public GitHubAnalyticsService(IDbContextFactory<GitHubConnectorContext> factory) => _factory = factory;

    public async Task<GitHubAnalyticsDto> GetAnalyticsAsync(int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var repos = await db.GitHubRepositories.Where(r => r.ModuleId == moduleId).ToListAsync();
        var releases = await db.GitHubReleases.Where(r => r.Repository.ModuleId == moduleId).ToListAsync();
        var issues = await db.GitHubIssues.Where(i => i.Repository.ModuleId == moduleId).ToListAsync();
        var actions = await db.GitHubActionWorkflows.Where(w => w.Repository.ModuleId == moduleId).ToListAsync();

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var monthlyReleases = releases
            .Where(r => r.PublishedAt >= sixMonthsAgo)
            .GroupBy(r => new { r.PublishedAt.Year, r.PublishedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyCount
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count()
            }).ToList();

        return new GitHubAnalyticsDto
        {
            TotalRepos = repos.Count,
            TotalStars = repos.Sum(r => r.Stars),
            TotalForks = repos.Sum(r => r.Forks),
            OpenIssues = issues.Count(i => i.State == "open"),
            TotalReleases = releases.Count,
            TotalWorkflowRuns = actions.Count,
            SuccessfulRuns = actions.Count(a => a.Conclusion == "success"),
            FailedRuns = actions.Count(a => a.Conclusion == "failure"),
            ReleasesPerMonth = monthlyReleases,
            TopRepos = repos.OrderByDescending(r => r.Stars).Take(5).Select(r => new RepoSummary
            {
                Name = r.FullName ?? r.Name,
                Stars = r.Stars,
                OpenIssues = r.OpenIssues,
                LatestRelease = releases.Where(rl => rl.RepositoryId == r.Id).MaxBy(rl => rl.PublishedAt)?.PublishedAt
            }).ToList()
        };
    }
}
