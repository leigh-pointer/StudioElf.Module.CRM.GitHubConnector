using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

/// <summary>
/// Entity builder for the <c>StudioElfCRMExtnGitHubRepo</c> table.
/// Creates columns matching <see cref="Models.GitHubRepository"/> properties.
/// </summary>
public class GitHubRepositoryEntityBuilder : AuditableBaseEntityBuilder<GitHubRepositoryEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubRepo";
    private readonly PrimaryKey<GitHubRepositoryEntityBuilder> _primaryKey =
        new("PK_StudioElfCRMExtnGitHubRepo", x => x.Id);

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubRepositoryEntityBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder instance.</param>
    /// <param name="database">The active database provider.</param>
    public GitHubRepositoryEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database)
        : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubRepositoryEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        ModuleId = AddIntegerColumn(table, "ModuleId");
        RepositoryId = AddLongColumn(table, "RepositoryId");
        Name = AddStringColumn(table, "Name", 250);
        FullName = AddStringColumn(table, "FullName", 500, true);
        Description = AddMaxStringColumn(table, "Description", true);
        Url = AddStringColumn(table, "Url", 1000, true);
        DefaultBranch = AddStringColumn(table, "DefaultBranch", 100, true);
        IsPrivate = AddBooleanColumn(table, "IsPrivate");
        PrimaryLanguage = AddStringColumn(table, "PrimaryLanguage", 100, true);
        Topics = AddStringColumn(table, "Topics", 2000, true);
        Stars = AddIntegerColumn(table, "Stars");
        Forks = AddIntegerColumn(table, "Forks");
        OpenIssues = AddIntegerColumn(table, "OpenIssues");
        LatestCommitAt = AddDateTimeColumn(table, "LatestCommitAt", true);
        LastSyncedOn = AddDateTimeColumn(table, "LastSyncedOn");
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ModuleId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Name { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> FullName { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Description { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Url { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> DefaultBranch { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> IsPrivate { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> PrimaryLanguage { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Topics { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Stars { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Forks { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> OpenIssues { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> LatestCommitAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> LastSyncedOn { get; set; } = null!;
}
