# MeetingAgent Agent Index

## Purpose

This file is a short index for agents working in this repository.

MeetingAgent is a host-private Microsoft Teams meeting facilitator with a companion Copilot experience. Phase 1 focuses on agenda planning, live pacing support, and post-meeting recap generation.

## Start Here

- Read `PLAN.md` for the product roadmap, phase boundaries, host workflow, architecture direction, and product scope.
- Read `TODO.md` for the implementation checklist, work breakdown, acceptance criteria, and phase-specific task tracking.
- Use this file for the high-level map of repo intent, system parts, and tool boundaries.

## Product Shape

- `Teams meeting app`: host setup, meeting side panel, and recap views.
- `Companion Copilot app`: natural-language host guidance before and after meetings.
- `ASP.NET Core web app`: UI host and HTTP API surface.
- `Background jobs`: transcript processing, recap generation, and retention cleanup.

## Core Rules

- The facilitator supports the host privately.
- The Phase 1 app is not a bot participant that joins the meeting like a person.
- Guidance should stay short and operational.
- Transcript-based insights must degrade gracefully when transcription is unavailable.
- Business logic, retention, and authorization live in code and services, not only in prompts.
- Minimal retention is the default.

## Core Domain

- `MeetingSession`
- `AgendaPlan`
- `AgendaSection`
- `FacilitatorAlert`
- `MeetingRecap`

## Core Services

- `AgendaDraftingService`
- `PacingEngine`
- `AlertService`
- `TranscriptProcessingService`
- `RecapGenerationService`
- `AuthorizationService`

## Tool Layer

These are the backend capabilities the Copilot experience uses through tools:

- `create_agenda_draft`
- `explain_pacing_risk`
- `suggest_host_intervention`
- `generate_host_recap`
- `get_meeting_status`

## Platform And Integration Tools

- `Microsoft Teams`: meeting side panel and meeting context.
- `Microsoft Entra ID`: authentication and host identity.
- `Microsoft Graph`: meeting metadata and transcript retrieval when available.
- `MCP tools`: tool calling path between the companion Copilot experience and backend capabilities.

## Request Flow Index

- Pre-meeting: capture goal, duration, and notes; draft agenda; host approval.
- In-meeting: load approved agenda; evaluate pacing; show private alerts and suggested interventions.
- Post-meeting: retrieve transcript when available; analyze drift and confusion; generate and store recap.

## Solution Layout

- `src/MeetingAgent.Web`
- `src/MeetingAgent.Application`
- `src/MeetingAgent.Domain`
- `src/MeetingAgent.Infrastructure`
- `src/MeetingAgent.Mcp`
- `tests/MeetingAgent.UnitTests`
- `tests/MeetingAgent.IntegrationTests`

## Agent Guidance

- Use `PLAN.md` when you need product intent, workflow context, phase definitions, or architecture framing.
- Use `TODO.md` when you need concrete implementation tasks, sequencing clues, or acceptance targets.
- Keep changes aligned with the host-private, Teams-native, transcript-first Phase 1 scope.
