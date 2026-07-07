using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Incoming GitHub webhook event logged for processing.
/// </summary>
[Table("StudioElfCRMExtnGitHubWebhookEvent")]
public class GitHubWebhookEvent : ModelBase
{
    [Key]
    public int Id { get; set; }
    public int ModuleId { get; set; }

    /// <summary>GitHub event type header (e.g. "push", "issues", "release").</summary>
    [Required, MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>Webhook action if applicable (e.g. "opened", "closed", "created").</summary>
    [MaxLength(100)]
    public string? Action { get; set; }

    /// <summary>Raw JSON payload from GitHub.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>GitHub delivery ID header.</summary>
    [MaxLength(200)]
    public string? DeliveryId { get; set; }

    /// <summary>Repository full name (owner/repo) that triggered the event.</summary>
    [MaxLength(500)]
    public string? RepositoryFullName { get; set; }

    /// <summary>Processing status: "new", "processed", "failed".</summary>
    [MaxLength(50)]
    public string Status { get; set; } = "new";

    /// <summary>When the event was received.</summary>
    public DateTime ReceivedOn { get; set; } = DateTime.UtcNow;

    /// <summary>When the event was successfully processed.</summary>
    public DateTime? ProcessedOn { get; set; }

    /// <summary>Error message if processing failed.</summary>
    public string? ErrorMessage { get; set; }
}
