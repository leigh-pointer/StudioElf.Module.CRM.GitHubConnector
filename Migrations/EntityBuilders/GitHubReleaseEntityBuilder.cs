using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

/// <summary>
/// Entity builder for the <c>StudioElfCRMExtnGitHubRelease</c> table.
/// Creates columns matching <see cref="Models.GitHubRelease"/> properties.
/// </summary>
public class GitHubReleaseEntityBuilder : AuditableBaseEntityBuilder<GitHubReleaseEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubRelease";
    private readonly PrimaryKey<GitHubReleaseEntityBuilder> _primaryKey =
        new("PK_StudioElfCRMExtnGitHubRelease", x => x.Id);

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubReleaseEntityBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder instance.</param>
    /// <param name="database">The active database provider.</param>
    public GitHubReleaseEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database)
        : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubReleaseEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        RepositoryId = AddIntegerColumn(table, "RepositoryId");
        ReleaseId = AddLongColumn(table, "ReleaseId");
        TagName = AddStringColumn(table, "TagName", 100, true);
        ReleaseName = AddStringColumn(table, "ReleaseName", 500, true);
        Body = AddMaxStringColumn(table, "Body", true);
        Url = AddStringColumn(table, "Url", 1000, true);
        IsPrerelease = AddBooleanColumn(table, "IsPrerelease");
        PublishedAt = AddDateTimeColumn(table, "PublishedAt");
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ReleaseId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> TagName { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ReleaseName { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Body { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Url { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> IsPrerelease { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> PublishedAt { get; set; } = null!;
}
