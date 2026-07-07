using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Repository.Databases.Interfaces;
using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Repository;

/// <summary>
/// Entity Framework Core database context for the GitHub Connector extension.
/// Manages GitHub repository, link, and release entities across multi-database providers.
/// </summary>
public class GitHubConnectorContext : DBContextBase, ITransientService, IMultiDatabase
{
    public GitHubConnectorContext(IDBContextDependencies DBContextDependencies)
        : base(DBContextDependencies) { }

    /// <summary>Tracked GitHub repositories.</summary>
    public DbSet<GitHubRepository> GitHubRepositories => Set<GitHubRepository>();

    /// <summary>Polymorphic links between repositories and CRM entities.</summary>
    public DbSet<GitHubRepositoryLink> GitHubRepositoryLinks => Set<GitHubRepositoryLink>();

    /// <summary>Synchronized GitHub releases.</summary>
    public DbSet<GitHubRelease> GitHubReleases => Set<GitHubRelease>();

    /// <summary>Synchronized GitHub issues and pull requests.</summary>
    public DbSet<GitHubIssue> GitHubIssues => Set<GitHubIssue>();

    /// <summary>Incoming webhook events.</summary>
    public DbSet<GitHubWebhookEvent> GitHubWebhookEvents => Set<GitHubWebhookEvent>();

    /// <summary>Synchronized GitHub Discussions.</summary>
    public DbSet<GitHubDiscussion> GitHubDiscussions => Set<GitHubDiscussion>();

    /// <summary>Synchronized GitHub Projects.</summary>
    public DbSet<GitHubProject> GitHubProjects => Set<GitHubProject>();

    /// <summary>Synchronized GitHub Actions workflow runs.</summary>
    public DbSet<GitHubActionWorkflow> GitHubActionWorkflows => Set<GitHubActionWorkflow>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<GitHubRepository>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubRepo"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(250).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.DefaultBranch).HasMaxLength(100);
            entity.Property(e => e.PrimaryLanguage).HasMaxLength(100);
            entity.Property(e => e.Topics).HasMaxLength(2000);

            // Prevent duplicate sync of the same GitHub repo within a module
            entity.HasIndex(e => new { e.RepositoryId, e.ModuleId }).IsUnique();
            entity.HasIndex(e => e.ModuleId);
        });

        builder.Entity<GitHubRepositoryLink>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubRepoLink"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();

            // Fast lookup by repository to find all linked entities
            entity.HasIndex(e => new { e.RepositoryId, e.EntityType, e.EntityId });
            // Fast reverse lookup by CRM entity to find all linked repos
            entity.HasIndex(e => new { e.EntityType, e.EntityId });

            entity.HasOne(e => e.Repository)
                  .WithMany()
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GitHubRelease>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubRelease"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TagName).HasMaxLength(100);
            entity.Property(e => e.ReleaseName).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(1000);

            // Prevent duplicate sync of the same release for a repo
            entity.HasIndex(e => new { e.ReleaseId, e.RepositoryId }).IsUnique();
            entity.HasIndex(e => e.RepositoryId);

            entity.HasOne(e => e.Repository)
                  .WithMany()
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GitHubIssue>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubIssue"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.HtmlUrl).HasMaxLength(1000);
            entity.Property(e => e.Labels).HasMaxLength(2000);
            entity.Property(e => e.UserLogin).HasMaxLength(200);
            entity.Property(e => e.MergeState).HasMaxLength(50);

            entity.HasIndex(e => new { e.IssueNumber, e.RepositoryId }).IsUnique();
            entity.HasIndex(e => e.RepositoryId);
            entity.HasIndex(e => new { e.RepositoryId, e.State });

            entity.HasOne(e => e.Repository)
                  .WithMany()
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GitHubWebhookEvent>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubWebhookEvent"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.DeliveryId).HasMaxLength(200);
            entity.Property(e => e.RepositoryFullName).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => e.ModuleId);
            entity.HasIndex(e => e.Status);
        });

        builder.Entity<GitHubDiscussion>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubDiscussion"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.HtmlUrl).HasMaxLength(1000);
            entity.Property(e => e.AuthorLogin).HasMaxLength(200);
            entity.HasIndex(e => new { e.DiscussionId, e.RepositoryId }).IsUnique();
            entity.HasIndex(e => e.RepositoryId);
            entity.HasOne(e => e.Repository)
                  .WithMany()
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GitHubProject>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubProject"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.HtmlUrl).HasMaxLength(1000);
            entity.HasIndex(e => new { e.ProjectId, e.RepositoryId }).IsUnique();
            entity.HasIndex(e => e.RepositoryId);
            entity.HasOne(e => e.Repository)
                  .WithMany()
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GitHubActionWorkflow>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnGitHubActionWorkflow"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkflowName).HasMaxLength(500);
            entity.Property(e => e.Branch).HasMaxLength(500);
            entity.Property(e => e.HeadBranch).HasMaxLength(500);
            entity.Property(e => e.HeadSha).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Conclusion).HasMaxLength(50);
            entity.Property(e => e.HtmlUrl).HasMaxLength(1000);
            entity.Property(e => e.TriggerEvent).HasMaxLength(100);
            entity.HasIndex(e => new { e.RunId, e.RepositoryId }).IsUnique();
            entity.HasIndex(e => e.RepositoryId);
            entity.HasOne(e => e.Repository)
                  .WithMany()
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

