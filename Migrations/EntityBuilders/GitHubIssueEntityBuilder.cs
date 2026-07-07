using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

public class GitHubIssueEntityBuilder : AuditableBaseEntityBuilder<GitHubIssueEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubIssue";
    private readonly PrimaryKey<GitHubIssueEntityBuilder> _primaryKey =
        new("PK_StudioElfCRMExtnGitHubIssue", x => x.Id);

    public GitHubIssueEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database)
        : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubIssueEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        RepositoryId = AddIntegerColumn(table, "RepositoryId");
        IssueNumber = AddLongColumn(table, "IssueNumber");
        Title = AddStringColumn(table, "Title", 500);
        Body = AddMaxStringColumn(table, "Body", true);
        State = AddStringColumn(table, "State", 50);
        Url = AddStringColumn(table, "Url", 1000, true);
        HtmlUrl = AddStringColumn(table, "HtmlUrl", 1000, true);
        Labels = AddStringColumn(table, "Labels", 2000, true);
        UserLogin = AddStringColumn(table, "UserLogin", 200, true);
        IsPullRequest = AddBooleanColumn(table, "IsPullRequest");
        MergeState = AddStringColumn(table, "MergeState", 50, true);
        CreatedAt = AddDateTimeColumn(table, "CreatedAt");
        UpdatedAt = AddDateTimeColumn(table, "UpdatedAt", true);
        ClosedAt = AddDateTimeColumn(table, "ClosedAt", true);
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> IssueNumber { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Title { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Body { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> State { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Url { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> HtmlUrl { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Labels { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> UserLogin { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> IsPullRequest { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> MergeState { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> CreatedAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> UpdatedAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ClosedAt { get; set; } = null!;
}
