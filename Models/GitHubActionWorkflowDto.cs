namespace StudioElf.Module.GitHubConnector.Models;

public class GitHubActionWorkflowDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public long RunId { get; set; }
    public string? WorkflowName { get; set; }
    public string? Branch { get; set; }
    public string Status { get; set; } = "";
    public string? Conclusion { get; set; }
    public string? HtmlUrl { get; set; }
    public int? RunNumber { get; set; }
    public string? TriggerEvent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GitHubDiscussionDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public long DiscussionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? Category { get; set; }
    public string State { get; set; } = "open";
    public string? HtmlUrl { get; set; }
    public string? AuthorLogin { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GitHubProjectDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string State { get; set; } = "open";
    public string? HtmlUrl { get; set; }
    public int? Number { get; set; }
    public DateTime CreatedAt { get; set; }
}

