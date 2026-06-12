# MeetingAgent

MeetingAgent is a host-private Microsoft Teams meeting facilitator with a companion MCP-backed Copilot experience. Phase 1 is a Teams-native, transcript-first internal pilot focused on agenda planning, live pacing support, and post-meeting recap generation.

## Current solution layout

- `src/MeetingAgent.Web`: Razor Pages host UI plus lightweight API endpoints for the Teams side-panel experience.
- `src/MeetingAgent.Application`: application-layer orchestration and tool contracts.
- `src/MeetingAgent.Domain`: domain-layer home for core meeting concepts.
- `src/MeetingAgent.Infrastructure`: configuration, retention defaults, and infrastructure registrations.
- `src/MeetingAgent.Mcp`: companion host for MCP-exposed backend tools.
- `tests/MeetingAgent.UnitTests`: unit tests for application behavior.
- `tests/MeetingAgent.IntegrationTests`: integration tests for the web host.

## Prerequisites

- `.NET SDK 10.0.x`
- PowerShell on Windows for the current local workflow
- A trusted local ASP.NET Core HTTPS certificate for browser testing

To trust the local development certificate:

```powershell
dotnet dev-certs https --trust
```

## Local setup

Restore, build, and test:

```powershell
dotnet restore MeetingAgent.slnx
dotnet build MeetingAgent.slnx --no-restore
dotnet test MeetingAgent.slnx --no-build
```

The default development database is SQL Server LocalDB:

```powershell
sqllocaldb info
dotnet ef database update `
  --project src/MeetingAgent.Infrastructure `
  --startup-project src/MeetingAgent.Web
```

Run the Teams-facing web host:

```powershell
dotnet run --project src/MeetingAgent.Web
```

Run the MCP host:

```powershell
dotnet run --project src/MeetingAgent.Mcp
```

## Project notes

- The web project uses Razor Pages for the page-centered host workflow and a small `/api` surface for meeting status.
- The MCP host currently exposes placeholder tool metadata and a health endpoint.
- Durable product state uses EF Core SQL Server migrations. Local development uses `MSSQLLocalDB`; deployed environments use Azure SQL through `Sql:ServerName` and `Sql:DatabaseName`.
- Retention defaults live in configuration and are intentionally conservative for the Phase 1 pilot.
- Raw transcript text is not stored in SQL. Transcript text should stay in transient processing or blob artifacts governed by retention, while SQL stores only metadata and derived recap/alert information.

## Storage configuration

The storage layer resolves configuration in this order:

1. `ConnectionStrings:MeetingAgent`
2. `Sql:ServerName` plus `Sql:DatabaseName`
3. Development fallback: `(localdb)\MSSQLLocalDB`

Retention settings:

- `MeetingAgent:Retention:TranscriptArtifactDays`
- `MeetingAgent:Retention:RecapArtifactDays`
- `MeetingAgent:Retention:MeetingMetadataDays`
- `MeetingAgent:RetentionCleanup:Enabled`
- `MeetingAgent:RetentionCleanup:IntervalMinutes`

Only the Web host registers the in-process retention cleanup worker. The MCP host can use storage but does not run the scheduled cleanup loop.

## Teams testing

Teams packaging, app manifest work, and tunnel setup are not configured yet. The next repo tasks are:

1. Add the Teams app package and manifest.
2. Decide the local callback strategy (`dev tunnels`, `ngrok`, or equivalent).
3. Wire Teams meeting context, authentication, and organizer authorization.
