# MeetingAgent Technical Decisions

## Phase 1 Initial Implementation Stack

This document records the confirmed technical stack for the first implementation of MeetingAgent.

### Application Platform

- `ASP.NET Core` on `.NET` for the primary web application and HTTP API surface.
- `Razor Pages` for the page-centered host workflow with a lightweight `/api` area in the web host.
- A `Teams meeting app` as the primary in-meeting host experience.
- `Copilot Studio` as the companion conversational experience.
- A separate `ASP.NET Core` MCP host to expose backend capabilities to the companion agent.

### Identity And Integration

- `Microsoft Entra ID` with `single-tenant` authentication for the initial internal pilot.
- `Microsoft Graph` for meeting metadata and transcript retrieval when available.

### Product Shape

- A web-hosted Teams side panel for pre-meeting setup, in-meeting pacing support, and post-meeting recap access.
- Shared `Domain`, `Application`, and `Infrastructure` projects to keep product logic separated from host surfaces.
- Background processing for transcript analysis, recap generation, and retention cleanup.

### Scope Notes

- Phase 1 is `Teams-native`, `transcript-first`, and `host-private`.
- The Phase 1 app is not a bot participant that joins a meeting like a user.
- Transcript-based insights must degrade gracefully when transcription is unavailable.
- Minimal retention is the default.

### Source Alignment

These decisions are consistent with the current product direction described in `PLAN.md` and `agents.md`.

## Phase 3 Storage And Analytics Direction

This document records the confirmed direction for grounding Phase 3 before implementation work begins.

### Storage Model

- Use a relational `EF Core` model for durable product state and analytics.
- Use `SQLite` for local development and integration tests.
- Keep the schema portable for a future `Azure SQL` or SQL Server deployment.
- Avoid provider-specific database behavior unless there is a documented need.
- Do not add durable raw transcript storage for Phase 3 analytics.

### Analytics Model

- Treat `MeetingSeries` as the future concept that groups related `MeetingSession` records for recurring meeting history.
- Store per-meeting derived analytics metrics such as overrun amount, pacing risk count, drift count, confusion signal count, agenda fit, transcript availability, and recap status.
- Store concise evidence snippets, labels, recap summaries, action items, and insight categories when transcript-derived analytics need durable context.
- Build recurring meeting, team-level, and reporting views from derived rollups rather than querying raw transcript text.
- Recompute or refresh rollups through application services or background jobs as the implementation matures.

### Retention And Privacy Defaults

- Minimal retention remains the default for the internal pilot and future Phase 3 analytics.
- Raw transcript text remains transient processing input and should expire according to transcript artifact retention settings.
- Analytics retention policies should separately cover per-meeting metrics, evidence snippets, and rollup history.
- Reporting should remain host-private by default, with any presenter or administrator views controlled by explicit role and policy checks.

### Source Alignment

This direction is based on the current roadmap plus Microsoft guidance for EF Core providers, EF Core testing approaches, SQLite provider limitations, and Azure data store selection.
