using StudioElf.Module.CRM.Models;

namespace StudioElf.Module.GitHubConnector.Services;

/// <summary>Builds a KnowledgeGraph from GitHub data for a CRM entity.</summary>
public interface IGitHubKnowledgeGraphService
{
    /// <summary>Build GitHub knowledge graph for a CRM entity (Contact, Company, Deal).</summary>
    Task<KnowledgeGraph> BuildAsync(string entityType, int entityId, int moduleId);
}
