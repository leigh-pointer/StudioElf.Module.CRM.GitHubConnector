using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using StudioElf.Module.CRM.GitHubConnector;
using StudioElf.Module.CRM.Services;
using StudioElf.Module.GitHubConnector.Repository;
using StudioElf.Module.GitHubConnector.Services;

namespace StudioElf.Module.GitHubConnector.Startup;

/// <summary>
/// Server startup class that registers all GitHub Connector services with DI.
/// Oqtane invokes <see cref="ConfigureServices"/> during application startup.
/// </summary>
public class ServerStartup : IServerStartup
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
        // ICrmExtension — singleton factory (no DI params)
        services.AddSingleton<ICrmExtension>(sp => new GitHubConnectorExtension());

        // Database
        services.AddDbContextFactory<GitHubConnectorContext>(opt => { }, ServiceLifetime.Transient);

        // GitHub API client — typed HttpClient for automatic lifetime management
        services.AddHttpClient<IGitHubApiClient, GitHubApiClient>(client =>
        {
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("StudioElfCRM-GitHubConnector", "1.0"));
        });

        // Business logic services
        services.AddScoped<IGitHubRepositoryService, GitHubRepositoryService>();
        services.AddScoped<IGitHubReleaseService, GitHubReleaseService>();
        services.AddScoped<IGitHubSyncService, GitHubSyncService>();

        // Background sync job — auto-registers with Oqtane Job Scheduler
        services.AddHostedService<GitHubSyncHostedService>();
    }

    /// <inheritdoc />
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // No middleware needed for this extension
    }

    /// <inheritdoc />
    public void ConfigureMvc(IMvcBuilder mvcBuilder)
    {
        // Controllers discovered by convention; no additional setup needed
    }
}
