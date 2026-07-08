using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.GitHubConnector.Migrations.EntityBuilders;

/// <summary>
/// Entity builder for the <c>StudioElfCRMExtnGitHubWebhookEvent</c> table.
/// Creates columns matching <see cref="Models.GitHubWebhookEvent"/> properties.
/// </summary>
public class GitHubWebhookEventEntityBuilder : AuditableBaseEntityBuilder<GitHubWebhookEventEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnGitHubWebhookEvent";
    private readonly PrimaryKey<GitHubWebhookEventEntityBuilder> _primaryKey = new("PK_StudioElfCRMExtnGitHubWebhookEvent", x => x.Id);

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubWebhookEventEntityBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder instance.</param>
    /// <param name="database">The active database provider.</param>
    public GitHubWebhookEventEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override GitHubWebhookEventEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        ModuleId = AddIntegerColumn(table, "ModuleId");
        EventType = AddStringColumn(table, "EventType", 100);
        Action = AddStringColumn(table, "Action", 100, true);
        Payload = AddMaxStringColumn(table, "Payload");
        DeliveryId = AddStringColumn(table, "DeliveryId", 200, true);
        RepositoryFullName = AddStringColumn(table, "RepositoryFullName", 500, true);
        Status = AddStringColumn(table, "Status", 50);
        ReceivedOn = AddDateTimeColumn(table, "ReceivedOn");
        ProcessedOn = AddDateTimeColumn(table, "ProcessedOn", true);
        ErrorMessage = AddMaxStringColumn(table, "ErrorMessage", true);
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ModuleId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> EventType { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Action { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Payload { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> DeliveryId { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> RepositoryFullName { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> Status { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ReceivedOn { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ProcessedOn { get; set; } = null!;
    public OperationBuilder<AddColumnOperation> ErrorMessage { get; set; } = null!;
}

