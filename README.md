# StudioElf CRM GitHub Enterprise Connector

The **StudioElf CRM GitHub Enterprise Connector** is the flagship open source integration extension for [StudioElf CRM](https://crm.studio-elf.net), demonstrating enterprise API integration, synchronization services, webhooks, dashboard widgets, AI enrichment, and Knowledge Graph providers in a real-world solution.

Built on the [StudioElf CRM Extension SDK](https://crm.studio-elf.net/extensions/extension-sdk), this connector proves that the CRM platform can integrate with enterprise-grade external systems. It serves as:

- A real-world enterprise integration example
- A reference implementation for extension developers
- A showcase for advanced SDK capabilities
- A blueprint for future commercial connectors (Microsoft Graph, Azure DevOps, Jira, Teams, SharePoint)

---

## Table of Contents

1. [Features](#features)
2. [GitHub Setup](#github-setup)
3. [Oqtane Installation](#oqtane-installation)
4. [Configuration](#configuration)
5. [Usage Guide](#usage-guide)
6. [Webhooks](#webhooks)
7. [API Reference](#api-reference)
8. [Architecture](#architecture)
9. [Project Structure](#project-structure)
10. [Development](#development)
11. [SDK Capabilities](#sdk-capabilities)

---

## Features

### Phase 1 — Core Integration
- **Repository Management** — Track GitHub repositories and link them to CRM Companies, Contacts, and Deals
- **Repository Synchronization** — Sync name, description, URL, stars, forks, language, topics, commit dates
- **Release Tracking** — Import version numbers, release notes, dates, pre-release status
- **Timeline Integration** — GitHub activity appears in CRM contact timeline

### Phase 2 — Issue Tracking & UI
- **Issue & Pull Request Tracking** — Sync open/closed issues and PRs with labels
- **Dashboard Widgets** — GitHub Overview, Recent Releases, Analytics
- **Contact Tabs** — GitHub Repos, Knowledge Graph, Analytics per contact

### Phase 3 — Events & AI
- **Webhook Integration** — Receive real-time GitHub events (push, issues, releases, PRs)
- **Knowledge Graph** — Repository, issue, and release nodes for CRM entities

### Phase 4 — Extended Data
- **GitHub Actions** — Workflow runs, status, conclusions
- **Discussions** — Repository discussions and categories
- **Projects** — GitHub Projects (classic) with state and metadata

### Phase 5 — Analytics
- **Release cadence charts** — Chart.js bar charts showing release frequency
- **Per-contact analytics** — Summary cards + trend data
- **GHE Support** — Configurable API URL for GitHub Enterprise Server

---

## GitHub Setup

### 1. Create a Personal Access Token

GitHub Enterprise Connector uses a Personal Access Token (PAT) for API authentication.

**Classic token (recommended):**
1. GitHub.com → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Set a name (e.g. "StudioElf CRM")
4. Select scopes:
   - `repo` — full access to private repositories
   - `public_repo` — public repositories only
   - `read:user` — validate the token
5. Click "Generate token"
6. **Copy the token immediately** — it starts with `ghp_`

**Fine-grained token:**
1. GitHub.com → Settings → Developer settings → Personal access tokens → Fine-grained tokens
2. Repository access: select the repos you want to sync
3. Permissions: Contents (read), Issues (read), Pull requests (read), Metadata (read)

### 2. Configure Webhooks (Optional)

For real-time event processing:

1. GitHub repository → Settings → Webhooks → Add webhook
2. Payload URL: `https://your-oqtane-instance/api/crm/github/webhook?moduleId=YOUR_MODULE_ID`
3. Content type: `application/json`
4. Secret: optional
5. Events: Issues, Pull requests, Releases, Pushes (select individually)
6. Click "Add webhook"

Events are stored in `StudioElfCRMExtnGitHubWebhookEvent` table for auditing.

### 3. Identify Repositories for Tracking

Decide which repositories to sync. Each repo is identified by its full name in `owner/repo` format (e.g. `dotnet/aspnetcore`, `StudioElf/CRM`).

---

## Oqtane Installation

### Repository Location

This project expects to reside alongside the Oqtane framework in your development root:

```
Development Root/
├── oqtane.framework/                        # Oqtane platform source
├── StudioElf.Module.CRM/                    
│   └── Extensions/
│       └── StudioElf.Module.CRM.GitHubConnector/   # this repo
├── [other modules]/ 
```

Clone into the `Extensions` folder:

```bash
cd <path-to>/StudioElf.Module.CRM/Extensions
git clone <repo-url>
```

The project references Oqtane DLLs via relative path `../../../oqtane.framework/` from this structure. The CRM DLLs are referenced from the Oqtane Server bin directory where the CRM module is installed.

### Prerequisites
- [Oqtane](https://www.oqtane.org/) 10.x
- [StudioElf CRM](https://crm.studio-elf.net) module installed
- .NET 10 SDK
- GitHub Personal Access Token

### Build
```bash
git clone <repo-url>
cd StudioElf.Module.CRM.GitHubConnector
dotnet build
```

The post-build target copies `StudioElf.Module.CRM.GitHubConnector.Oqtane.dll` to the Oqtane Server bin directory at `oqtane.framework/Oqtane.Server/bin/Debug/net10.0/`.

### Restart
Restart the Oqtane application to load the extension. The migration runs automatically on first startup, creating all 8 database tables.

---

## Configuration

### Host Settings (gear icon)
Access: CRM → Extensions tab → gear icon next to "CRM GitHub Enterprise Connector"

| Setting | Description |
|---------|-------------|
| **GitHub API URL** | API base URL. Default: `https://api.github.com`. For GitHub Enterprise Server: `https://[hostname]/api/v3` |
| **Personal Access Token** | GitHub PAT with `repo` scope. Stored encrypted in module settings. |

Only Host users can access these settings.

### User Settings (Settings tab)
Access: Open GitHub extension → Settings vertical tab

| Setting | Description |
|---------|-------------|
| **Sync Interval** | Minutes between automatic background syncs. Default: 30 |
| **Enable Release Tracking** | Sync releases from tracked repos |
| **Enable Issue Tracking** | Sync issues and pull requests |
| **Show Pre-releases** | Include pre-release versions in widgets |
| **Dashboard Widgets** | Toggle visibility for GitHub Overview, Recent Releases, Analytics |

---

## Usage Guide

### Adding a Repository
1. Open the GitHub extension from the CRM tab bar
2. Click "+ Add Repository"
3. Enter `owner/repo` (e.g. `dotnet/aspnetcore`)
4. Click "Add"

The extension fetches metadata (stars, forks, language, description) from GitHub immediately.

### Synchronizing Data
Click "Sync All" to:
- Update repository metadata (stars, forks, language, etc.)
- Sync releases (if enabled)
- Sync issues and pull requests (if enabled)

The background job also syncs automatically at the configured interval.

### Linking to a Contact
1. Open a Contact detail page
2. Click the "GitHub Repos" vertical tab
3. Select a repository from the dropdown
4. Click "Link"

The repository is now associated with the contact and appears in:
- Contact timeline (releases, issues)
- Knowledge Graph tab
- Analytics tab

### Unlinking
In the "GitHub Repos" tab, click "Unlink" next to any linked repository.

### Viewing Knowledge Graph
The Knowledge Graph tab shows a visual representation of:
- Repositories linked to the contact (root)
- Open issues under each repository
- Recent releases under each repository

### Viewing Analytics
The Analytics tab shows:
- Summary cards: total releases, open issues, stars
- Release cadence bar chart (6-month view, Chart.js)
- Per-repository breakdown with latest release dates

---

## Webhooks

### Incoming Webhook Endpoint

```
POST /api/crm/github/webhook?moduleId={id}
```

This public endpoint receives GitHub webhook events. No authentication required.

### Supported Events
| Event | Processing |
|-------|------------|
| `push` | Stored in webhook event table |
| `issues` | Stored in webhook event table |
| `release` | Stored in webhook event table |
| `pull_request` | Stored in webhook event table |

All events are logged to the `StudioElfCRMExtnGitHubWebhookEvent` table with full payload, delivery ID, and status.

### GitHub Configuration
1. Repository → Settings → Webhooks → Add webhook
2. Payload URL: `https://your-server/api/crm/github/webhook?moduleId={moduleId}`
3. Content type: `application/json`
4. Secret: optional (HMAC-SHA256 validation planned)
5. Which events: send me everything, or select individual events

---

## API Reference

Base URL: `/api/crm/github/`

All endpoints (except webhook) require `?moduleId={id}` and `ViewModule` or `EditModule` authorization.

### Repositories
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/repositories?moduleId=` | View | List all tracked repos |
| GET | `/repositories/{id}?moduleId=` | View | Get single repo |
| POST | `/repositories?moduleId=` | Edit | Add repo `{fullName: "owner/repo"}` |
| DELETE | `/repositories/{id}?moduleId=` | Edit | Remove tracked repo |
| POST | `/repositories/sync?moduleId=` | Edit | Sync all repos + releases + issues |
| POST | `/repositories/{id}/sync?moduleId=` | Edit | Sync single repo |

### Releases
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/releases?moduleId=&repositoryId=` | View | Releases for a repo |
| GET | `/releases/recent?moduleId=&count=` | View | Recent releases across all repos |
| GET | `/releases/entity?moduleId=&entityType=&entityId=` | View | Releases linked to a CRM entity |

### Issues
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/issues?moduleId=&repositoryId=&state=` | View | Issues for a repo |
| GET | `/issues/entity?moduleId=&entityType=&entityId=` | View | Issues linked to a CRM entity |

### Links
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/links?moduleId=&repositoryId=` | View | Links for a repo |
| POST | `/links?moduleId=` | Edit | Create link `{repositoryId, entityType, entityId}` |
| DELETE | `/links/{id}?moduleId=` | Edit | Remove link |
| GET | `/entity/{entityType}/{entityId}?moduleId=` | View | Repos linked to entity |

### Actions, Discussions, Projects
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/actions?moduleId=&repositoryId=` | View | Workflow runs |
| POST | `/actions/sync?moduleId=&repositoryId=` | Edit | Sync workflow runs |
| GET | `/discussions?moduleId=&repositoryId=` | View | Discussions |
| POST | `/discussions/sync?moduleId=&repositoryId=` | Edit | Sync discussions |
| GET | `/projects?moduleId=&repositoryId=` | View | Projects |
| POST | `/projects/sync?moduleId=&repositoryId=` | Edit | Sync projects |

### Analytics & Knowledge Graph
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/analytics?moduleId=` | View | Aggregated analytics |
| GET | `/knowledgegraph?moduleId=&entityType=&entityId=` | View | Knowledge Graph for entity |

### Webhook
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/webhook?moduleId=` | Anonymous | Receive GitHub webhook events |

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
              │ Core │      │  UI      │
              │  DB  │      │  + Charts │
              └──────┘      └──────────┘
```

### Data Flow
1. **Sync trigger**: User clicks "Sync All" or background job timer fires
2. **Settings load**: PAT and feature toggles read from module settings
3. **GitHub API calls**: Each tracked repo is fetched via GitHub REST API v3
4. **Database upsert**: Results stored in extension tables (upsert by GitHub ID)
5. **UI refresh**: Widgets and tabs re-query the database

### Background Job
`GitHubSyncHostedService` extends Oqtane's `HostedServiceBase`:
- Frequency: configurable (default 30 minutes)
- Auto-registers with Oqtane Job Scheduler on first run
- Tenant-aware execution
- Job logging with retention

### Database
All tables use the `StudioElfCRMExtn` prefix with `ModelBase` audit columns:

| Table | Entity | Purpose |
|-------|--------|---------|
| `StudioElfCRMExtnGitHubRepo` | GitHubRepository | Tracked repositories |
| `StudioElfCRMExtnGitHubRepoLink` | GitHubRepositoryLink | Polymorphic links to CRM entities (Company/Contact/Deal) |
| `StudioElfCRMExtnGitHubRelease` | GitHubRelease | Synced releases |
| `StudioElfCRMExtnGitHubIssue` | GitHubIssue | Issues and pull requests |
| `StudioElfCRMExtnGitHubWebhookEvent` | GitHubWebhookEvent | Incoming webhook events |
| `StudioElfCRMExtnGitHubDiscussion` | GitHubDiscussion | Synced discussions |
| `StudioElfCRMExtnGitHubProject` | GitHubProject | Synced projects |
| `StudioElfCRMExtnGitHubActionWorkflow` | GitHubActionWorkflow | Synced workflow runs |

Migration is a single file `01000000_Initialize.cs` that creates all 8 tables. Re-installing the module drops and recreates all tables.

---

## Project Structure

```
Controllers/
  GitHubController.cs            # REST API (20+ endpoints)
Extensions/
  GitHubConnectorExtension.cs    # ICrmExtension contract
Client/
  GitHubConnectorBase.cs         # Base class (inherits CrmBase)
  GitHubConnectorShell.razor     # Main shell UI (repo management + sync)
  GitHubContactTab.razor         # Contact detail tab (link/unlink repos)
  GitHubContactAnalyticsTab.razor# Contact analytics (Chart.js)
  GitHubKnowledgeGraphTab.razor  # Knowledge Graph visualization
  GitHubHostSettings.razor       # Host/Admin settings (PAT, API URL)
  GitHubUserSettings.razor       # User settings (toggles, widgets)
  ChartInterop.cs                # Chart.js JS interop
  Components/
    GitHubRepositoriesList.razor # Reusable repo list component
    GitHubReleasesList.razor     # Reusable release list component
  GitHubOverviewWidget.razor     # Dashboard widget
  GitHubRecentReleasesWidget.razor# Dashboard widget
  GitHubAnalyticsWidget.razor    # Dashboard widget
Services/
  IGitHubApiClient.cs            # GitHub API client interface
  GitHubApiClient.cs             # HTTP client with rate-limit handling
  IGitHubRepositoryService.cs    # Repository CRUD + sync
  GitHubRepositoryService.cs
  IGitHubReleaseService.cs       # Release sync + query
  GitHubReleaseService.cs
  IGitHubIssueService.cs         # Issue/PR sync + query
  GitHubIssueService.cs
  IGitHubSyncService.cs          # Orchestration sync service
  GitHubSyncService.cs
  IGitHubWebhookService.cs       # Webhook event processing
  GitHubWebhookService.cs
  IGitHubTimelineService.cs      # Timeline items for CRM
  GitHubTimelineService.cs
  IGitHubActionService.cs        # Actions workflow runs
  GitHubActionService.cs
  IGitHubDiscussionService.cs    # Discussion sync
  GitHubDiscussionService.cs
  IGitHubProjectService.cs       # Project sync
  GitHubProjectService.cs
  IGitHubAnalyticsService.cs     # Aggregated analytics
  GitHubAnalyticsService.cs
  IGitHubKnowledgeGraphService.cs# Knowledge Graph builder
  GitHubKnowledgeGraphService.cs
  GitHubSyncHostedService.cs     # Background sync job
Models/                          # 8 entities + DTOs + settings
Migrations/
  01000000_Initialize.cs         # Single migration, 8 tables
  EntityBuilders/                # 8 entity builders
Repository/
  GitHubConnectorContext.cs      # EF Core DbContext
Startup/
  ServerStartup.cs               # DI registration
Manager/
  GitHubConnectorManager.cs      # Module install/uninstall
ModuleInfo.cs                    # Oqtane module registration
```

---

## Development

### Adding a New Entity

1. Create model in `Models/` extending `ModelBase`
2. Create entity builder in `Migrations/EntityBuilders/`
3. Add `DbSet<T>` in `Repository/GitHubConnectorContext.cs`
4. Add `OnModelCreating` configuration
5. Add builder to `Migrations/01000000_Initialize.cs` Up/Down
6. Create service interface + implementation in `Services/`
7. Register service in `Startup/ServerStartup.cs`
8. Add API endpoints in `Controllers/GitHubController.cs`

### Code Standards

- All public types, methods, and properties require XML doc comments
- All DateTime values use `DateTime.UtcNow`
- FK cascade paths: Module → NoAction, Entity → Cascade
- Controllers use `[Route(ControllerRoutes.ApiRoute)]`
- Public endpoints have `[AllowAnonymous]` + `[IgnoreAntiforgeryToken]`
- Background jobs extend `HostedServiceBase`

### Building
```bash
dotnet build
```

The `CopyToOqtane` target copies the DLL to the Oqtane framework bin directory automatically.

---

## SDK Capabilities Demonstrated

| Capability | Status | Details |
|------------|--------|---------|
| Custom Entities | ✅ | 8 tables with ModelBase audit columns |
| Custom Tabs | ✅ | 3 contact sub-tabs (Repos, KG, Analytics) |
| Dashboard Widgets | ✅ | Overview, Recent Releases, Analytics |
| Settings | ✅ | Host settings (gear icon) + User settings (tab) |
| Background Services | ✅ | GitHubSyncHostedService (Oqtane Job Scheduler) |
| Webhooks | ✅ | Public endpoint, event storage |
| Timeline Integration | ✅ | GetTimelineItems for Contact/Company/Deal |
| Knowledge Graph | ✅ | Repository + Issue + Release nodes |
| AI Integration | ✅ | Data feeds via timeline entries |
| External API Integration | ✅ | GitHub REST API v3 with rate limiting |
| OAuth Authentication | ✅ | Personal Access Token |

---

## License

MIT — Open source.

Built on [Oqtane](https://www.oqtane.org/) and [StudioElf CRM](https://crm.studio-elf.net).
