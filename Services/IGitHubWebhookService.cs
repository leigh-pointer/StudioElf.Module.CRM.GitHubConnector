namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Processes incoming GitHub webhook events.
/// </summary>
public interface IGitHubWebhookService
{
    /// <summary>Process a webhook payload: validate signature, store event, trigger actions.</summary>
    Task<WebhookResult> ProcessAsync(string eventType, string payload, string? signature, int moduleId);
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

