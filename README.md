# StudioElf CRM GitHub Enterprise Connector

The **StudioElf CRM GitHub Enterprise Connector** is the flagship open source integration extension for [StudioElf CRM](https://crm.studio-elf.net), demonstrating enterprise API integration, synchronization services, webhooks, dashboard widgets, AI enrichment, and Knowledge Graph providers in a real-world solution.

Built on the [StudioElf CRM Extension SDK](https://crm.studio-elf.net/extensions/extension-sdk), this connector proves that the CRM platform can integrate with enterprise-grade external systems. It serves as:

- A real-world enterprise integration example
- A reference implementation for extension developers
- A showcase for advanced SDK capabilities
- A blueprint for future commercial connectors (Microsoft Graph, Azure DevOps, Jira, Teams, SharePoint)

---

## Features

### Phase 1 — Core Integration
- **Repository Management** — Track GitHub repositories and link them to CRM Companies, Contacts, and Deals
- **Repository Synchronization** — Sync name, description, URL, stars, forks, language, topics, commit dates
- **Release Tracking** — Import version numbers, release notes, dates, pre-release status
- **Timeline Integration** — GitHub activity (releases published, issues opened) appears in CRM timeline

### Phase 2 — Issue Tracking & UI
- **Issue & Pull Request Tracking** — Sync open/closed issues and PRs with labels, assignees, milestones
- **Dashboard Widgets** — GitHub Overview, Recent Releases, Analytics widgets on CRM dashboard
- **Contact Tab** — View and manage linked repositories directly from Contact detail page

### Phase 3 — Events & AI
- **Webhook Integration** — Receive real-time GitHub events (push, issues, releases, PRs) via public endpoint
- **AI Context** — GitHub data feeds into CRM AI enrichment for smarter context-aware insights

### Phase 4 — Extended Data
- **GitHub Actions** — Sync workflow runs, status, conclusions, and triggers
- **Discussions** — Track repository discussions and categories
- **Projects** — Sync GitHub Projects (classic) with state and metadata

### Phase 5 — Analytics
- **Analytics Dashboard** — Release cadence, issue resolution rates, workflow pass/fail metrics
- **GHE Support** — Configurable API URL for GitHub Enterprise Server

---

## Architecture

```
┌─────────────┐     ┌──────────────────┐     ┌──────────────┐
│  GitHub API  │────▶│  Extension Core  │────▶│  CRM SDK     │
│  REST v3     │     │  (Services)      │     │  (ICrmExt.)  │
└─────────────┘     └──────────────────┘     └──────────────┘
                           │
                    ┌──────┴──────┐
                    │              │
              ┌──────┐      ┌──────────┐
              │  EF  │      │  Blazor  │
              │ Core │      │  Shell   │
              │  DB  │      │  + Widgets│
              └──────┘      └──────────┘
```

### Database

All tables use `StudioElfCRMExtn` prefix with `ModelBase` audit columns:

| Table | Entity | Purpose |
|-------|--------|---------|
| `StudioElfCRMExtnGitHubRepo` | GitHubRepository | Tracked repositories |
| `StudioElfCRMExtnGitHubRepoLink` | GitHubRepositoryLink | Polymorphic links to CRM entities |
| `StudioElfCRMExtnGitHubRelease` | GitHubRelease | Synced releases |
| `StudioElfCRMExtnGitHubIssue` | GitHubIssue | Issues and pull requests |
| `StudioElfCRMExtnGitHubWebhookEvent` | GitHubWebhookEvent | Incoming webhook events |
| `StudioElfCRMExtnGitHubDiscussion` | GitHubDiscussion | Synced discussions |
| `StudioElfCRMExtnGitHubProject` | GitHubProject | Synced projects |
| `StudioElfCRMExtnGitHubActionWorkflow` | GitHubActionWorkflow | Synced workflow runs |

---

## Getting Started

### Prerequisites
- [Oqtane](https://www.oqtane.org/) 10.x
- [StudioElf CRM](https://crm.studio-elf.net) module installed
- .NET 10 SDK
- GitHub Personal Access Token with `repo` scope

### Build & Install
```bash
dotnet build
```
DLL auto-copies to Oqtane framework bin. Restart Oqtane.

### Configuration
1. **Host Settings**: CRM → Extensions tab → gear icon → enter API URL + PAT
2. **User Settings**: Open GitHub extension → Settings tab → configure toggles
3. **Add Repo**: Click "+ Add Repository" → enter `owner/name` (e.g. `dotnet/aspnetcore`)
4. **Sync**: Click "Sync All" → fetches metadata, releases, issues

### Linking to CRM
Open Contact detail page → "GitHub Repos" tab → select repo → Link.

---

## API Reference

Base: `/api/crm/github/`

| Group | Endpoints |
|-------|-----------|
| **Repos** | `GET/POST /repositories`, `DELETE /repositories/{id}`, `POST /repositories/sync`, `POST /repositories/{id}/sync` |
| **Releases** | `GET /releases`, `GET /releases/recent`, `GET /releases/entity` |
| **Issues** | `GET /issues`, `GET /issues/entity` |
| **Links** | `GET/POST /links`, `DELETE /links/{id}`, `GET /entity/{type}/{id}` |
| **Actions** | `GET /actions`, `POST /actions/sync` |
| **Discussions** | `GET /discussions`, `POST /discussions/sync` |
| **Projects** | `GET /projects`, `POST /projects/sync` |
| **Analytics** | `GET /analytics` |
| **Webhook** | `POST /webhook` (public, `[AllowAnonymous]`) |

All authenticated endpoints use `?moduleId={id}` and require ViewModule/EditModule policy.

---

## SDK Capabilities Demonstrated

| Capability | Status |
|------------|--------|
| Custom Entities | ✅ 8 tables |
| Custom Tabs | ✅ Contact detail tab |
| Dashboard Widgets | ✅ 3 widgets |
| Settings | ✅ Host settings (gear icon) + User settings (tab) |
| Background Services | ✅ GitHubSyncHostedService |
| Webhooks | ✅ Receive endpoint |
| Timeline Integration | ✅ GetTimelineItems |
| AI Integration | ✅ Data feeds into AI context |
| External API Integration | ✅ GitHub REST API v3 |
| OAuth Authentication | ✅ PAT (Phase 1) |

---

## Project Structure

```
Controllers/GitHubController.cs      # REST API (20+ endpoints)
Extensions/GitHubConnectorExtension.cs  # ICrmExtension contract
Client/
  ├── GitHubConnectorShell.razor      # Main UI
  ├── GitHubContactTab.razor          # Contact detail tab
  ├── GitHubHostSettings.razor        # Host/Admin settings
  ├── GitHubUserSettings.razor        # User settings
  ├── GitHubOverviewWidget.razor      # Dashboard widget
  ├── GitHubRecentReleasesWidget.razor# Dashboard widget
  ├── GitHubAnalyticsWidget.razor     # Dashboard widget
  └── GitHubConnectorBase.cs          # Component base
Services/                             # 10 interfaces + implementations
Models/                               # 8 entities + DTOs + settings
Migrations/01000000_Initialize.cs     # Single migration, 8 tables
Repository/GitHubConnectorContext.cs  # EF Core DbContext
Startup/ServerStartup.cs              # DI registration
Manager/GitHubConnectorManager.cs     # Module install/uninstall
ModuleInfo.cs                         # Oqtane module registration
```

---

## License

MIT — Open source. Built on [Oqtane](https://www.oqtane.org/) and [StudioElf CRM](https://crm.studio-elf.net).
