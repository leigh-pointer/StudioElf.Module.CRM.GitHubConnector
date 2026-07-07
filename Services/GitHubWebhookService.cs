using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StudioElf.Module.GitHubConnector.Models;
using StudioElf.Module.GitHubConnector.Repository;

namespace StudioElf.Module.GitHubConnector.Services;

public class GitHubWebhookService : IGitHubWebhookService
{
    private readonly IDbContextFactory<GitHubConnectorContext> _contextFactory;
    private readonly ILogger<GitHubWebhookService> _logger;

    public GitHubWebhookService(
        IDbContextFactory<GitHubConnectorContext> contextFactory,
        ILogger<GitHubWebhookService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<WebhookResult> ProcessAsync(string eventType, string payload, string? signature, int moduleId)
    {
        try
        {
            string? repoFullName = null;
            try
            {
                var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("repository", out var repo))
                    repoFullName = repo.TryGetProperty("full_name", out var name) ? name.GetString() : null;
            }
            catch { /* partial payload OK */ }

            await using var db = await _contextFactory.CreateDbContextAsync();
            var webhookEvent = new GitHubWebhookEvent
            {
                ModuleId = moduleId,
                EventType = eventType,
                Payload = payload,
                DeliveryId = null,
                RepositoryFullName = repoFullName,
                Status = "new",
                ReceivedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
            };
            db.GitHubWebhookEvents.Add(webhookEvent);
            await db.SaveChangesAsync();

            _logger.LogInformation("Webhook {EventType} stored (Id: {Id}, repo: {Repo})",
                eventType, webhookEvent.Id, repoFullName);

            return new WebhookResult
            {
                Success = true,
                Message = $"Event {eventType} received.",
                EventId = webhookEvent.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing failed");
            return new WebhookResult { Success = false, Message = ex.Message };
        }
    }
}
