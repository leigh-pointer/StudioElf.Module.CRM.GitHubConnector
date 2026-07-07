using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Migrations;

/// <summary>
/// Initial migration for the GitHub Connector extension.
/// Creates the GitHubRepository, GitHubRepositoryLink, and GitHubRelease tables.
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var releaseBuilder = new GitHubReleaseEntityBuilder(migrationBuilder, ActiveDatabase);
        releaseBuilder.Drop();

        var linkBuilder = new GitHubRepositoryLinkEntityBuilder(migrationBuilder, ActiveDatabase);
        linkBuilder.Drop();

        var repoBuilder = new GitHubRepositoryEntityBuilder(migrationBuilder, ActiveDatabase);
        repoBuilder.Drop();
    }
}
