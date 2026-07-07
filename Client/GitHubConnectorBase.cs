using Oqtane.Shared;
using StudioElf.Module.CRM;

namespace StudioElf.Module.GitHubConnector.Client;
/// <summary>
/// Base class for the Extension module, providing common functionality for all components.
/// All Razor components in the extension module should inherit from this class to ensure 
/// consistent behavior and access to shared services and are ignored by Oqtane's module system.
/// </summary>
[OqtaneIgnore]
public class GitHubConnectorBase : CrmBase
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

}