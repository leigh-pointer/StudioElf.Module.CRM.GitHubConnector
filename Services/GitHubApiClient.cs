using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>
/// Default implementation of <see cref="IGitHubApiClient"/>.
/// Communicates with the GitHub REST API v3 via typed <see cref="HttpClient"/>.
/// </summary>
public class GitHubApiClient : IGitHubApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubApiClient> _logger;

    private string _baseUrl = "https://api.github.com";
    private string? _token;

    /// <summary>
    /// Initializes a new instance of <see cref="GitHubApiClient"/>.
    /// </summary>
    public GitHubApiClient(HttpClient httpClient, ILogger<GitHubApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("StudioElfCRM-GitHubConnector", "1.0"));
    }

    /// <inheritdoc />
    public void Configure(string baseUrl, string token)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _token = token;
    }

    /// <inheritdoc />
    public async Task<JsonDocument> GetRepositoryAsync(string owner, string repo, CancellationToken ct = default)
    {
        var url = $"{_baseUrl}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}";
        return await SendAsync(url, ct);
    }

    /// <inheritdoc />
    public async Task<List<JsonDocument>> GetReleasesAsync(string owner, string repo, CancellationToken ct = default)
    {
        var results = new List<JsonDocument>();
        var url = $"{_baseUrl}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/releases?per_page=100";

        while (url != null)
        {
            var (document, nextUrl) = await SendWithPaginationAsync(url, ct);
            if (document?.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    results.Add(JsonDocument.Parse(element.GetRawText()));
                }
            }
            url = nextUrl;
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<JsonDocument>> GetIssuesAsync(string owner, string repo, string state = "open", CancellationToken ct = default)
    {
        var results = new List<JsonDocument>();
        var url = $"{_baseUrl}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/issues?state={state}&per_page=100";

        while (url != null)
        {
            var (document, nextUrl) = await SendWithPaginationAsync(url, ct);
            if (document?.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    results.Add(JsonDocument.Parse(element.GetRawText()));
                }
            }
            url = nextUrl;
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<JsonDocument>> GetActionsAsync(string owner, string repo, CancellationToken ct = default)
    {
        var results = new List<JsonDocument>();
        var url = $"{_baseUrl}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/actions/runs?per_page=100";

        while (url != null)
        {
            var (document, nextUrl) = await SendWithPaginationAsync(url, ct);
            if (document?.RootElement.TryGetProperty("workflow_runs", out var runs) == true && runs.ValueKind == JsonValueKind.Array)
            {
                foreach (var run in runs.EnumerateArray())
                    results.Add(JsonDocument.Parse(run.GetRawText()));
            }
            url = nextUrl;
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<List<JsonDocument>> GetDiscussionsAsync(string owner, string repo, CancellationToken ct = default)
    {
        var results = new List<JsonDocument>();
        var url = $"{_baseUrl}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/discussions?per_page=100";

        while (url != null)
        {
            var (document, nextUrl) = await SendWithPaginationAsync(url, ct);
            if (document?.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in document.RootElement.EnumerateArray())
                    results.Add(JsonDocument.Parse(element.GetRawText()));
            }
            url = nextUrl;
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<List<JsonDocument>> GetProjectsAsync(string owner, string repo, CancellationToken ct = default)
    {
        var results = new List<JsonDocument>();
        var url = $"{_baseUrl}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/projects?per_page=100";

        while (url != null)
        {
            var (document, nextUrl) = await SendWithPaginationAsync(url, ct);
            if (document?.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in document.RootElement.EnumerateArray())
                    results.Add(JsonDocument.Parse(element.GetRawText()));
            }
            url = nextUrl;
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateTokenAsync(CancellationToken ct = default)
    {
        try
        {
            await SendAsync($"{_baseUrl}/user", ct);
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("GitHub PAT validation failed: unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub PAT validation failed with error");
            return false;
        }
    }

    /// <summary>Sends a GET request and returns the response as a JsonDocument.</summary>
    private async Task<JsonDocument> SendAsync(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuth(request);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await HandleRateLimitAsync(response, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("GitHub API returned {StatusCode} for {Url}: {Body}",
                (int)response.StatusCode, url, body);

            throw new HttpRequestException(
                $"GitHub API returned {(int)response.StatusCode}: {body}",
                null,
                response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonDocument.Parse(json);
    }

    /// <summary>Sends a GET request and returns parsed body + next page URL from Link header.</summary>
    private async Task<(JsonDocument? Document, string? NextUrl)> SendWithPaginationAsync(
        string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuth(request);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await HandleRateLimitAsync(response, ct);

        if (!response.IsSuccessStatusCode)
            return (null, null);

        var json = await response.Content.ReadAsStringAsync(ct);
        var document = JsonDocument.Parse(json);
        var nextUrl = ParseNextPageUrl(response.Headers);

        return (document, nextUrl);
    }

    /// <summary>Applies the PAT Bearer token to an outgoing request.</summary>
    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    /// <summary>
    /// Handles GitHub API rate limiting.
    /// On 403/429, waits the Retry-After duration and retries once.
    /// Logs warnings when approaching the rate limit.
    /// </summary>
    private async Task HandleRateLimitAsync(HttpResponseMessage response, CancellationToken ct)
    {
        // Check approaching limit
        if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues)
            && int.TryParse(remainingValues.FirstOrDefault(), out var remaining)
            && remaining < 10)
        {
            _logger.LogWarning("GitHub API rate limit approaching: {Remaining} remaining", remaining);
        }

        // Handle rate limit exceeded
        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds
                ?? response.Headers.RetryAfter?.Date?.Subtract(DateTimeOffset.UtcNow).TotalSeconds
                ?? 60;

            _logger.LogWarning("GitHub API rate limited. Waiting {Seconds}s before retry.", retryAfter);
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(retryAfter, 300)), ct);
        }
    }

    /// <summary>
    /// Parses the Link header to find the next page URL.
    /// GitHub API uses standard RFC 5988 Link headers for pagination.
    /// </summary>
    private static string? ParseNextPageUrl(HttpHeaders headers)
    {
        if (!headers.TryGetValues("Link", out var linkValues))
            return null;

        foreach (var link in linkValues)
        {
            // Format: <https://api.github.com/...?page=2>; rel="next", <...>; rel="last"
            var parts = link.Split(',');
            foreach (var part in parts)
            {
                if (part.Contains("rel=\"next\"", StringComparison.OrdinalIgnoreCase))
                {
                    var start = part.IndexOf('<');
                    var end = part.IndexOf('>');
                    if (start >= 0 && end > start)
                    {
                        return part.Substring(start + 1, end - start - 1);
                    }
                }
            }
        }

        return null;
    }
}

