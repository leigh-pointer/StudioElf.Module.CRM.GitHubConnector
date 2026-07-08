# StudioElf CRM GitHub Enterprise Connector

The **StudioElf CRM GitHub Enterprise Connector** is the flagship open source integration extension for StudioElf CRM, demonstrating enterprise API integration, synchronization services, webhooks, dashboard widgets, AI enrichment, and Knowledge Graph providers in a real-world solution.

---

## What It Does

The connector bridges GitHub and StudioElf CRM, turning development activity into CRM intelligence. Track repositories, sync releases, monitor issues, and link everything to your contacts, companies, and deals — all without leaving the CRM.

**For sales teams:** See which customers are affected by a new release. Know when a client's repository has open issues that need attention. Prepare for account meetings with a complete picture of development activity.

**For support teams:** Link support contacts to their repositories. Get timeline visibility when releases ship. Monitor issue activity for key accounts.

**For engineering:** Sync GitHub metadata into the CRM automatically. View release cadence charts. Track workflow run success rates. Get a Knowledge Graph of repositories, issues, and releases for any contact.

## Key Capabilities

- **Repository sync** — Track any public or private repository. Sync stars, forks, language, topics, and commit activity on demand or via scheduled background jobs.
- **Release tracking** — Import version numbers, release notes, and publish dates. View release cadence in Chart.js-powered bar charts.
- **Issue & PR monitoring** — Sync open and closed issues with labels. Separate issues from pull requests. All visible on the contact detail page.
- **Entity linking** — Link repositories to any CRM entity. A contact gets a "GitHub Repos" tab showing linked repos, open issues, and releases. The CRM timeline shows GitHub events alongside emails and activities.
- **Dashboard widgets** — At-a-glance widgets on the CRM dashboard: GitHub Overview (stars, forks, issues), Recent Releases, and Analytics (release frequency, workflow pass/fail rates).
- **Webhook receiver** — Public endpoint for real-time GitHub events. Push, issues, releases, and pull request events are stored and visible in the UI.
- **Knowledge Graph** — Query GitHub data as a structured graph of Repository, Issue, and Release nodes connected to CRM entities via the Knowledge Graph API.
- **Background sync** — Automatic synchronization via Oqtane's job scheduler. Configurable interval, tenant-aware, self-healing.
- **GitHub Enterprise Server** — Configurable API URL for self-hosted GHE instances.

## What It Demonstrates

This extension is a reference implementation for the StudioElf CRM Extension SDK. Every major SDK capability is exercised:

| SDK Feature | How It's Used |
|-------------|---------------|
| Custom Entities | 8 database tables with EF Core migrations |
| Dashboard Widgets | 3 interactive widgets on the CRM dashboard |
| Contact Tabs | 3 sub-tabs (Repos, Knowledge Graph, Analytics) |
| Timeline Integration | GitHub events appear in CRM contact timeline |
| Settings | Host settings (gear icon) + user settings (tab) |
| Background Services | Scheduled sync via Oqtane HostedServiceBase |
| Webhooks | Public endpoint with event storage |
| Knowledge Graph | Repository + Issue + Release nodes for any entity |
| External APIs | Full GitHub REST API v3 integration with rate limiting |

## Architecture Overview

```
GitHub API  →  Extension Services  →  EF Core Database  →  CRM UI (Blazor)
                     ↓                         ↓
              Background Sync           Dashboard Widgets
              Webhook Receiver         Contact Tabs
                                        Knowledge Graph API
```

The connector is a single-assembly Oqtane module built with .NET 10. It registers itself with the CRM via the `ICrmExtension` interface and provides its own Blazor components for the shell, widgets, and contact tabs.

## Getting Started

1. Generate a GitHub Personal Access Token with `repo` scope
2. Build `dotnet build` and restart Oqtane
3. Configure the PAT in CRM → Extensions → gear icon
4. Add a repository by `owner/name`
5. Click Sync All

Full documentation is available in the project [README](../README.md).
