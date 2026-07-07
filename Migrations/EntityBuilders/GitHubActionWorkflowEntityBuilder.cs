using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

public class GitHubActionWorkflowEntityBuilder : AuditableBaseEntityBuilder<GitHubActionWorkflowEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubActionWorkflow";
    private readonly PrimaryKey<GitHubActionWorkflowEntityBuilder> _primaryKey = new("PK_StudioElfCRMExtnGitHubActionWorkflow", x => x.Id);

    public GitHubActionWorkflowEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubActionWorkflowEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        RepositoryId = AddIntegerColumn(table, "RepositoryId");
        RunId = AddLongColumn(table, "RunId");
        WorkflowName = AddStringColumn(table, "WorkflowName", 500, true);
        Branch = AddStringColumn(table, "Branch", 500, true);
        HeadBranch = AddStringColumn(table, "HeadBranch", 500, true);
        HeadSha = AddStringColumn(table, "HeadSha", 100, true);
        Status = AddStringColumn(table, "Status", 50);
        Conclusion = AddStringColumn(table, "Conclusion", 50, true);
        HtmlUrl = AddStringColumn(table, "HtmlUrl", 1000, true);
        RunNumber = AddIntegerColumn(table, "RunNumber", true);
        TriggerEvent = AddStringColumn(table, "TriggerEvent", 100, true);
        CreatedAt = AddDateTimeColumn(table, "CreatedAt");
        UpdatedAt = AddDateTimeColumn(table, "UpdatedAt", true);
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RunId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> WorkflowName { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Branch { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> HeadBranch { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> HeadSha { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Status { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Conclusion { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> HtmlUrl { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RunNumber { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> TriggerEvent { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> CreatedAt { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> UpdatedAt { get; set; } = null!;
}
