using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

/// <summary>
/// Entity builder for the <c>StudioElfCRMExtnGitHubRepoLink</c> table.
/// Creates columns matching <see cref="Models.GitHubRepositoryLink"/> properties.
/// </summary>
public class GitHubRepositoryLinkEntityBuilder : AuditableBaseEntityBuilder<GitHubRepositoryLinkEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubRepoLink";
    private readonly PrimaryKey<GitHubRepositoryLinkEntityBuilder> _primaryKey =
        new("PK_StudioElfCRMExtnGitHubRepoLink", x => x.Id);

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubRepositoryLinkEntityBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder instance.</param>
    /// <param name="database">The active database provider.</param>
    public GitHubRepositoryLinkEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database)
        : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubRepositoryLinkEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        RepositoryId = AddIntegerColumn(table, "RepositoryId");
        EntityType = AddStringColumn(table, "EntityType", 50);
        EntityId = AddIntegerColumn(table, "EntityId");
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> EntityType { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> EntityId { get; set; } = null!;
}
