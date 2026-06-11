# Meeting Facilitator Agent Roadmap

## Summary

Build a Microsoft Teams meeting facilitator that helps the meeting organizer keep discussion on topic, finish within the allocated time, and notice when participants may be confused or overloaded.

The product is a Teams meeting app with a Copilot Studio companion agent. It is designed first for a single internal Microsoft 365 tenant, with a long-term architecture that can grow into a true live facilitator.

The product has two layers:

- A meeting-native Teams app experience for setup, live host guidance, and recap.
- An AI orchestration layer using Copilot Studio, custom backend services, and MCP tools for reasoning, recommendations, and meeting intelligence.

The roadmap is intentionally phased. Phase 1 should be Teams-native and transcript-first. Phase 2 should add lower-latency meeting understanding once the project is ready for custom real-time audio or transcription infrastructure.

## Product Definition

### Primary User

The first audience is the meeting organizer or host. The facilitator should help the person responsible for running the meeting, rather than trying to serve every participant equally in the initial version.

### First Supported Environment

The first supported environment is Microsoft Teams meetings.

### Core Behaviors

- Topic discipline: compare the conversation against the approved goal and agenda, then identify when the meeting is drifting.
- Time discipline: track elapsed time, remaining time, and whether the meeting can still finish cleanly.
- Comprehension awareness: identify signals that a concept may be unclear, too complex, or repeatedly misunderstood.

### Agent Response Style

The facilitator should support the host privately. It should not interrupt the meeting automatically or behave like an autonomous participant.

The agent should flag issues and suggest next steps. Suggestions should be short, specific, and operational, such as:

- Restate the last point in simpler terms.
- Ask the room for a quick confirmation before moving on.
- Summarize the decision being made.
- Park an off-topic thread for follow-up.
- Move to the next agenda item to protect the meeting end time.

## Host Workflow

### Pre-Meeting

The host enters a meeting goal, meeting duration, and optional notes.

The agent generates a draft agenda or facilitation plan. The host reviews and edits that draft before the meeting begins.

The approved agenda becomes the source of truth for topic and pacing evaluation.

### In-Meeting

The host opens the facilitator as a Teams meeting side panel.

The side panel shows:

- Current meeting status.
- Elapsed and remaining time.
- Agenda context.
- Pacing guidance.
- Private host-facing alerts.
- Suggested interventions.

The host may also chat with the companion Copilot Studio agent in Teams for explanations, recap help, or follow-up questions.

### Post-Meeting

The product produces a brief host recap that highlights:

- Timing issues.
- Topic drift moments.
- Confusion or overload moments.
- Follow-up actions.
- Facilitation suggestions for the next meeting.

## How The Organizer Adds It To A Teams Meeting

The facilitator should be delivered as a Teams app optimized for meetings.

### Organizer Entry Flow

1. The organizer installs the facilitator app from the Teams app store, or IT preinstalls it for target users in the tenant.
2. Before the meeting, the organizer opens the Teams meeting details and adds the facilitator app using the meeting's `Add a tab (+)` flow.
3. During the meeting, the organizer can also add or open the app from the meeting toolbar or `More > Add an app`.
4. Once added, the app appears as a meeting side panel.
5. The side panel becomes the primary in-meeting facilitator experience.

### Companion Agent Flow

The Copilot Studio agent should be published to Teams as a companion surface.

The host can install and chat with the agent separately in Teams. If allowed by tenant and Teams configuration, the agent can also be added to meeting chat for chat-based interaction.

For the initial architecture, the product should not be described as a bot participant that joins the call and listens like a person. It is a Teams meeting app plus a companion agent.

## Architecture Roadmap

### Phase 1: Transcript-First Meeting Assistant

Phase 1 should deliver the full host workflow without requiring custom real-time audio infrastructure.

Key capabilities:

- Goal intake.
- Draft agenda generation.
- Approved meeting plan.
- Teams meeting side panel.
- Live pacing dashboard.
- Agenda-based host guidance.
- Post-meeting transcript analysis.
- Brief host recap.

Copilot Studio should provide the facilitator persona and conversational guidance. MCP-backed tools should expose backend capabilities for agenda generation, intervention suggestions, and recap generation.

Transcript intelligence should be treated as primarily post-meeting in Phase 1 because Microsoft Teams custom app transcript access is generally available after transcription artifacts are published. During the meeting, the app should still provide useful live pacing and agenda-based guidance.

### Phase 2: Near-Real-Time Facilitator Intelligence

Phase 2 should add a custom Teams meeting ingestion path for lower-latency meeting understanding.

Likely additions:

- Teams calling or media bot infrastructure.
- Azure Speech or equivalent speech-to-text services.
- Rolling transcript windows.
- Near-real-time topic drift detection.
- Near-real-time confusion and overload detection.
- Alert deduplication so the host is not repeatedly nudged about the same issue.

Phase 2 should reuse the same side-panel experience, alert model, agenda model, and host-facing recommendation style from Phase 1.

### Phase 3: Meeting Operating System

Phase 3 should expand the product from single-meeting assistance to recurring meeting quality improvement.

Possible capabilities:

- Recurring meeting analytics.
- Repeated overrun detection.
- Common confusion theme tracking.
- Agenda quality feedback.
- Team-specific facilitation styles.
- Admin-configurable policies.
- Optional role-based experiences for hosts, presenters, and administrators.

Minimal retention should remain the default unless explicit governance or analytics requirements justify broader storage.

## Key Interfaces And Product Contracts

### Core Product Concepts

- MeetingSession: the identity, schedule, organizer, and status of a meeting instance.
- AgendaPlan: the approved meeting goal, structure, timing plan, and version.
- FacilitatorAlert: a private host-facing issue with type, severity, evidence, and suggested response.
- MeetingRecap: the post-meeting output summarizing what helped or hindered the meeting.

### AI Responsibilities

Copilot Studio owns:

- Facilitator persona.
- Natural-language interaction with the host.
- Companion chat experience in Teams.

Custom backend services own:

- Meeting state.
- Agenda persistence.
- Pacing calculations.
- Transcript processing.
- Scoring logic.
- Policy enforcement.
- Retention behavior.

MCP tools expose backend capabilities to Copilot Studio, including:

- Agenda drafting.
- Pacing risk explanation.
- Host intervention suggestions.
- Meeting recap generation.

### Teams Responsibilities

The Teams meeting app owns:

- Setup experience.
- Meeting side-panel user interface.
- Meeting-context access.
- Host-facing live guidance.

Teams app management owns:

- Installation.
- Tenant approval.
- User availability.
- Meeting app distribution.

The Copilot Studio companion agent is optional for the side-panel workflow but recommended for conversational access before and after meetings.

## Data Handling Defaults

The system should use minimal retention by default.

Recommended defaults:

- Persist approved agendas, meeting metadata, facilitator alerts, and recap artifacts.
- Treat raw transcript text as transient processing input where possible.
- Avoid storing raw transcript data longer than necessary.
- If transcription is unavailable or disabled, continue supporting agenda planning and pacing while marking transcript-based insights as unavailable.

## Success Scenarios

### Product Fit

- A host can prepare a meeting in a few minutes and leave with an approved agenda.
- A host can open the facilitator inside a Teams meeting without extra operational setup.
- The facilitator improves meeting discipline without becoming a disruptive participant.

### Core Scenarios

- The discussion drifts from the agenda and the host receives a private return-to-topic recommendation.
- The meeting is running long and the host receives a pacing warning with a suggested recovery action.
- The transcript shows repeated questions, clarification loops, or high complexity and the host receives a comprehension warning.
- A completed meeting produces a short recap the host can use immediately.

### Roadmap Gates

- Phase 1 is successful when hosts can reliably use the meeting app in Teams and receive useful pacing guidance plus post-meeting intelligence.
- Phase 2 is successful when transcript-derived topic and confusion alerts can be raised during the meeting with acceptable latency and low noise.
- Phase 3 is successful when the product helps teams improve meeting quality across repeated sessions, not just within a single meeting.

## Assumptions And Defaults

- The first deployment is an internal pilot, not a multi-tenant SaaS launch.
- The first audience is meeting organizers, not all participants equally.
- The primary in-meeting surface is the Teams side panel.
- The host provides a goal and the system drafts the agenda.
- Invite-text inference is secondary.
- The initial product uses minimal retention.
- The long-term vision includes a true live facilitator.
- The first practical architecture is Teams-native, transcript-first, and host-private.

