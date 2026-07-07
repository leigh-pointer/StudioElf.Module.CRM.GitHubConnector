using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Migrations;

/// <summary>
/// Initial migration for the GitHub Connector extension.
/// Creates all extension tables: repos, links, releases, issues, webhooks, discussions, projects, actions.
/// </summary>
[DbContext(typeof(GitHubConnectorContext))]
[Migration("StudioElf.Module.GitHubConnector.01.00.00.00")]
public class InitializeGitHubConnector : MultiDatabaseMigration
{
    /// <summary>
    /// Initializes a new instance of <see cref="InitializeGitHubConnector"/>.
    /// </summary>
    /// <param name="database">The active database provider instance.</param>
    public InitializeGitHubConnector(IDatabase database)
        : base(database)
    {
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var repoBuilder = new GitHubRepositoryEntityBuilder(migrationBuilder, ActiveDatabase);
        repoBuilder.Create();

        var linkBuilder = new GitHubRepositoryLinkEntityBuilder(migrationBuilder, ActiveDatabase);
        linkBuilder.Create();

        var releaseBuilder = new GitHubReleaseEntityBuilder(migrationBuilder, ActiveDatabase);
        releaseBuilder.Create();

        var issueBuilder = new GitHubIssueEntityBuilder(migrationBuilder, ActiveDatabase);
        issueBuilder.Create();

        var webhookBuilder = new GitHubWebhookEventEntityBuilder(migrationBuilder, ActiveDatabase);
        webhookBuilder.Create();

        var discussionBuilder = new GitHubDiscussionEntityBuilder(migrationBuilder, ActiveDatabase);
        discussionBuilder.Create();

        var projectBuilder = new GitHubProjectEntityBuilder(migrationBuilder, ActiveDatabase);
        projectBuilder.Create();

        var actionBuilder = new GitHubActionWorkflowEntityBuilder(migrationBuilder, ActiveDatabase);
        actionBuilder.Create();
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var releaseBuilder = new GitHubReleaseEntityBuilder(migrationBuilder, ActiveDatabase);
        releaseBuilder.Drop();

        var linkBuilder = new GitHubRepositoryLinkEntityBuilder(migrationBuilder, ActiveDatabase);
        linkBuilder.Drop();

        var repoBuilder = new GitHubRepositoryEntityBuilder(migrationBuilder, ActiveDatabase);
        repoBuilder.Drop();

        var issueBuilder = new GitHubIssueEntityBuilder(migrationBuilder, ActiveDatabase);
        issueBuilder.Drop();

        var webhookBuilder = new GitHubWebhookEventEntityBuilder(migrationBuilder, ActiveDatabase);
        webhookBuilder.Drop();

        var discussionBuilder = new GitHubDiscussionEntityBuilder(migrationBuilder, ActiveDatabase);
        discussionBuilder.Drop();

        var projectBuilder = new GitHubProjectEntityBuilder(migrationBuilder, ActiveDatabase);
        projectBuilder.Drop();

        var actionBuilder = new GitHubActionWorkflowEntityBuilder(migrationBuilder, ActiveDatabase);
        actionBuilder.Drop();
    }
}
