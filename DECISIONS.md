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
