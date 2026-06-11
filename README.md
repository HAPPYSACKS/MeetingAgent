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
- Retention defaults live in configuration and are intentionally conservative for the Phase 1 pilot.

## Teams testing

Teams packaging, app manifest work, and tunnel setup are not configured yet. The next repo tasks are:

1. Add the Teams app package and manifest.
2. Decide the local callback strategy (`dev tunnels`, `ngrok`, or equivalent).
3. Wire Teams meeting context, authentication, and organizer authorization.
