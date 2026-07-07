namespace StudioElf.Module.GitHubConnector.Models;

/// <summary>
/// Data transfer object for <see cref="GitHubRelease"/>.
/// Includes resolved repository name for display.
/// </summary>
public class GitHubReleaseDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }

    /// <summary>Resolved repository full name (e.g. "dotnet/aspnetcore").</summary>
    public string RepositoryName { get; set; } = string.Empty;
    public long ReleaseId { get; set; }
    public string? TagName { get; set; }
    public string? ReleaseName { get; set; }
    public string? Body { get; set; }
    public string? Url { get; set; }
    public bool IsPrerelease { get; set; }
    public DateTime PublishedAt { get; set; }
}
