using System.Text.Json;
using Microsoft.Extensions.Logging;
using Oqtane.Services;
using Oqtane.Shared;
using StudioElf.Module.GitHubConnector.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Default implementation of <see cref="IGitHubSyncService"/>.
/// Reads settings, configures the API client, and orchestrates sync across repos and releases.
/// </summary>
public class GitHubSyncService : IGitHubSyncService
{
    private readonly IGitHubRepositoryService _repositoryService;
    private readonly IGitHubReleaseService _releaseService;
    private readonly IGitHubIssueService _issueService;
    private readonly ISettingService _settingService;
    private readonly ILogger<GitHubSyncService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubSyncService"/>.
    /// </summary>
    public GitHubSyncService(
        IGitHubRepositoryService repositoryService,
        IGitHubReleaseService releaseService,
        IGitHubIssueService issueService,
        ISettingService settingService,
        ILogger<GitHubSyncService> logger)
    {
        _repositoryService = repositoryService;
        _releaseService = releaseService;
        _issueService = issueService;
        _settingService = settingService;
        _logger = logger;
    }

    public async Task<SyncResultDto> SyncAllAsync(int moduleId, CancellationToken ct = default)
    {
        var result = new SyncResultDto();

        try
        {
            // Load and apply settings
            var settings = await LoadSettingsAsync(moduleId);

            if (string.IsNullOrEmpty(settings.PersonalAccessToken))
            {
                result.Success = false;
                result.Message = "GitHub Personal Access Token is not configured. Please add it in the extension settings.";
                return result;
            }

            // Configure API client with current settings
            // Note: API client is scoped, configured per-sync-call to pick up setting changes

            // Sync repositories
            var repoResult = await _repositoryService.SyncAllAsync(moduleId, ct);
            result.RepositoriesUpdated = repoResult.RepositoriesUpdated;
            result.Errors.AddRange(repoResult.Errors);

            if (ct.IsCancellationRequested)
            {
                result.Message = $"Sync cancelled. Repositories synced: {result.RepositoriesUpdated}.";
                return result;
            }

            // Sync releases if enabled
            if (settings.EnableReleaseTracking)
            {
                // Get all repos to sync their releases
                var repos = await _repositoryService.GetAllAsync(moduleId);

                foreach (var repo in repos)
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        var releaseCount = await _releaseService.SyncReleasesAsync(
                            repo.Id, moduleId, ct);
                        result.ReleasesUpdated += releaseCount;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync releases for repository {RepoId}", repo.Id);
                        result.Errors.Add($"Release sync failed for '{repo.FullName}': {ex.Message}");
                        result.Success = false;
                    }
                }
            }

            // Sync issues if enabled
            if (settings.EnableIssueTracking)
            {
                var repos = await _repositoryService.GetAllAsync(moduleId);

                foreach (var repo in repos)
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        var issueCount = await _issueService.SyncIssuesAsync(
                            repo.Id, moduleId, ct);
                        result.IssuesUpdated += issueCount;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync issues for repository {RepoId}", repo.Id);
                        result.Errors.Add($"Issue sync failed for '{repo.FullName}': {ex.Message}");
                        result.Success = false;
                    }
                }
            }

            result.Success = result.Errors.Count == 0;
            result.Message = $"Sync completed. " +
                $"{result.RepositoriesUpdated} repos, {result.ReleasesUpdated} releases, {result.IssuesUpdated} issues synced. " +
                $"{result.Errors.Count} error(s).";
        }
        catch (OperationCanceledException)
        {
            result.Message = $"Sync cancelled. {result.RepositoriesUpdated} repos processed.";
            result.Success = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub sync failed for module {ModuleId}", moduleId);
            result.Success = false;
            result.Errors.Add($"Sync failed: {ex.Message}");
            result.Message = $"Sync failed: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Loads extension settings from the CRM module settings system.
    /// Settings stored as JSON in a single module setting key.
    /// </summary>
    private async Task<GitHubSettings> LoadSettingsAsync(int moduleId)
    {
        var settings = await _settingService.GetSettingsAsync(EntityNames.Module, moduleId);
        var json = settings.GetValueOrDefault(GitHubSettings.SettingsKey, "{}");
        return JsonSerializer.Deserialize<GitHubSettings>(json) ?? new GitHubSettings();
    }
}

