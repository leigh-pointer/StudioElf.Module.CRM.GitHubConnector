using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

/// <summary>
/// Entity builder for the <c>StudioElfCRMExtnGitHubProject</c> table.
/// Creates columns matching <see cref="Models.GitHubProject"/> properties.
/// </summary>
public class GitHubProjectEntityBuilder : AuditableBaseEntityBuilder<GitHubProjectEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubProject";
    private readonly PrimaryKey<GitHubProjectEntityBuilder> _primaryKey = new("PK_StudioElfCRMExtnGitHubProject", x => x.Id);

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubProjectEntityBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder instance.</param>
    /// <param name="database">The active database provider.</param>
    public GitHubProjectEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubProjectEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        RepositoryId = AddIntegerColumn(table, "RepositoryId");
        ProjectId = AddLongColumn(table, "ProjectId");
        Name = AddStringColumn(table, "Name", 500);
        Body = AddMaxStringColumn(table, "Body", true);
        State = AddStringColumn(table, "State", 50);
        HtmlUrl = AddStringColumn(table, "HtmlUrl", 1000, true);
        Number = AddIntegerColumn(table, "Number", true);
        CreatedAt = AddDateTimeColumn(table, "CreatedAt");
        UpdatedAt = AddDateTimeColumn(table, "UpdatedAt", true);
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ProjectId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Name { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Body { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> State { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> HtmlUrl { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Number { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> CreatedAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> UpdatedAt { get; set; } = null!;
}

