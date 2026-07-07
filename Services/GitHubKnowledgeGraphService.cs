using Microsoft.EntityFrameworkCore;
using StudioElf.Module.CRM.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubKnowledgeGraphService : IGitHubKnowledgeGraphService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _factory;

    public GitHubKnowledgeGraphService(IDbContextFactory<GitHubConnectorContext> factory) => _factory = factory;

    public async Task<KnowledgeGraph> BuildAsync(string entityType, int entityId, int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var graph = new KnowledgeGraph
        {
            RootNodeId = $"crm:{entityType}:{entityId}",
            Metadata = new() { { "source", "GitHubConnector" } }
        };

        // Get repos linked to this entity
        var linkedRepoIds = await db.GitHubRepositoryLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .Select(l => l.RepositoryId)
            .ToListAsync();

        if (linkedRepoIds.Count == 0) return graph;

        var repos = await db.GitHubRepositories
            .Where(r => linkedRepoIds.Contains(r.Id))
            .ToListAsync();

        foreach (var repo in repos)
        {
            var repoNodeId = $"github:repo:{repo.Id}";
            graph.Nodes.Add(new KnowledgeNode
            {
                Id = repoNodeId,
                Type = "Repository",
                Name = repo.FullName ?? repo.Name,
                Properties = new()
                {
                    ["url"] = repo.Url ?? "",
                    ["stars"] = repo.Stars,
                    ["language"] = repo.PrimaryLanguage ?? "",
                    ["private"] = repo.IsPrivate
                }
            });

            graph.Edges.Add(new KnowledgeEdge
            {
                FromId = graph.RootNodeId,
                ToId = repoNodeId,
                Relationship = "has_repository"
            });

            // Add open issues for this repo
            var issues = await db.GitHubIssues
                .Where(i => i.RepositoryId == repo.Id && i.State == "open")
                .Take(20)
                .ToListAsync();

            foreach (var issue in issues)
            {
                var issueNodeId = issue.IsPullRequest ? $"github:pr:{issue.Id}" : $"github:issue:{issue.Id}";
                graph.Nodes.Add(new KnowledgeNode
                {
                    Id = issueNodeId,
                    Type = issue.IsPullRequest ? "PullRequest" : "Issue",
                    Name = $"#{issue.IssueNumber}: {issue.Title}",
                    Properties = new()
                    {
                        ["url"] = issue.HtmlUrl ?? "",
                        ["state"] = issue.State,
                        ["number"] = issue.IssueNumber
                    }
                });

                graph.Edges.Add(new KnowledgeEdge
                {
                    FromId = repoNodeId,
                    ToId = issueNodeId,
                    Relationship = issue.IsPullRequest ? "has_pull_request" : "has_issue"
                });
            }

            // Add recent releases
            var releases = await db.GitHubReleases
                .Where(r => r.RepositoryId == repo.Id)
                .OrderByDescending(r => r.PublishedAt)
                .Take(5)
                .ToListAsync();

            foreach (var release in releases)
            {
                var releaseNodeId = $"github:release:{release.Id}";
                graph.Nodes.Add(new KnowledgeNode
                {
                    Id = releaseNodeId,
                    Type = "Release",
                    Name = release.TagName ?? release.ReleaseName ?? "unknown",
                    Properties = new()
                    {
                        ["url"] = release.Url ?? "",
                        ["published"] = release.PublishedAt.ToString("yyyy-MM-dd"),
                        ["prerelease"] = release.IsPrerelease
                    }
                });

                graph.Edges.Add(new KnowledgeEdge
                {
                    FromId = repoNodeId,
                    ToId = releaseNodeId,
                    Relationship = "has_release"
                });
            }
        }

        return graph;
    }
}
