using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

/// <summary>
/// Entity builder for the <c>StudioElfCRMExtnGitHubDiscussion</c> table.
/// Creates columns matching <see cref="Models.GitHubDiscussion"/> properties.
/// </summary>
public class GitHubDiscussionEntityBuilder : AuditableBaseEntityBuilder<GitHubDiscussionEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubDiscussion";
    private readonly PrimaryKey<GitHubDiscussionEntityBuilder> _primaryKey = new("PK_StudioElfCRMExtnGitHubDiscussion", x => x.Id);

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubDiscussionEntityBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder instance.</param>
    /// <param name="database">The active database provider.</param>
    public GitHubDiscussionEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubDiscussionEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        RepositoryId = AddIntegerColumn(table, "RepositoryId");
        DiscussionId = AddLongColumn(table, "DiscussionId");
        Title = AddStringColumn(table, "Title", 500);
        Body = AddMaxStringColumn(table, "Body", true);
        Category = AddStringColumn(table, "Category", 100, true);
        State = AddStringColumn(table, "State", 50);
        HtmlUrl = AddStringColumn(table, "HtmlUrl", 1000, true);
        AuthorLogin = AddStringColumn(table, "AuthorLogin", 200, true);
        CreatedAt = AddDateTimeColumn(table, "CreatedAt");
        UpdatedAt = AddDateTimeColumn(table, "UpdatedAt", true);
        AnsweredAt = AddDateTimeColumn(table, "AnsweredAt", true);
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> DiscussionId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Title { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Body { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Category { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> State { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> HtmlUrl { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> AuthorLogin { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> CreatedAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> UpdatedAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> AnsweredAt { get; set; } = null!;
}

