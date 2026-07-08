namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Processes incoming GitHub webhook events.
/// </summary>
public interface IGitHubWebhookService
{
    /// <summary>Process a webhook payload: validate signature, store event, trigger actions.</summary>
    Task<WebhookResult> ProcessAsync(string eventType, string payload, string? signature, int moduleId);

    /// <summary>Get recent webhook events for a module.</summary>
    Task<List<WebhookEventDto>> GetRecentEventsAsync(int moduleId, int count = 50);
}

/// <summary>DTO for displaying a webhook event in the UI.</summary>
public class WebhookEventDto
{
    /// <summary>Event ID.</summary>
    public int Id { get; set; }
    /// <summary>GitHub event type (push, issues, release).</summary>
    public string EventType { get; set; } = "";
    /// <summary>Repository full name that triggered the event.</summary>
    public string? RepositoryFullName { get; set; }
    /// <summary>Processing status: new, processed, ignored, failed.</summary>
    public string Status { get; set; } = "";
    /// <summary>When the event was received.</summary>
    public DateTime ReceivedOn { get; set; }
}

/// <summary>Result of webhook processing.</summary>
public class WebhookResult
{
    /// <summary>True if the webhook was processed successfully.</summary>
    public bool Success { get; set; }
    /// <summary>Human-readable result message.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>ID of the stored webhook event.</summary>
    public int EventId { get; set; }
}

