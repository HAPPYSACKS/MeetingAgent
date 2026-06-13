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

The repository includes a Teams meeting app package template under `teams/appPackage/` and a packaging script for local Microsoft Teams testing through a Microsoft dev tunnel.

Before running `zip.ps1`, create a local `.env` file from the checked-in example and set the public dev tunnel URL:

```powershell
Copy-Item .env.example .env
notepad .env
```

Minimum `.env` contents:

```text
MEETINGAGENT_TEAMS_BASE_URL=https://<your-dev-tunnel-host>
```

The `.env` file is ignored by git and should contain machine- or tenant-specific package values.

Start the web host with the HTTPS launch profile:

```powershell
dotnet run --project src/MeetingAgent.Web --launch-profile https
```

Use `teams/README.md` for the full dev tunnel, package generation, organizer install, and tenant approval flow.

Graph transcript retrieval is tracked as a later implementation slice.

## Authentication configuration

The web host has repo-side Microsoft Entra single-tenant authentication wiring for the Phase 1 pilot. Tenant app registration is still an external setup step; do not commit tenant secrets.

Configure these settings through user secrets, environment variables, or deployment settings:

```text
AzureAd__Instance=https://login.microsoftonline.com/
AzureAd__TenantId=<tenant-id>
AzureAd__ClientId=<app-registration-client-id>
AzureAd__Domain=<tenant-domain>
AzureAd__CallbackPath=/signin-oidc
AzureAd__SignedOutCallbackPath=/signout-callback-oidc
Teams__ApplicationIdUri=api://<lowercase-dev-tunnel-or-host>/<app-registration-client-id>
Teams__AllowedTenantId=<tenant-id>
Teams__ValidDomains__0=<lowercase-dev-tunnel-or-host>
```

The protected API surface accepts Teams SSO bearer tokens and interactive Entra cookie sign-in. `/health`, `/api/status`, `/Privacy`, `/Terms`, and `/Teams/Configure` remain anonymous. Use `/api/auth/me` to verify the current sanitized identity and `/api/auth/meetings/{meetingId}/host-access` to verify host-only meeting access.

For local SSO testing, the matching Entra app registration must expose
`api://<dev-tunnel-host>/<client-id>`, define the `access_as_user` scope,
preauthorize Teams desktop/mobile and web clients, and enable ID tokens under
the Web authentication platform for fallback browser sign-in.
