# Meeting Facilitator Agent TODO

This checklist turns `PLAN.md` into implementation work. The initial build target is a Phase 1 internal pilot: a Teams-native, transcript-first, host-private meeting facilitator with a Teams side panel and Copilot Studio companion agent.

## 0. Foundation Decisions

- [x] Confirm the technical stack for the first implementation.
- [x] Use ASP.NET Core and .NET for the custom app and backend.
- [ ] Use a Teams meeting app as the primary in-meeting surface.
- [ ] Use Copilot Studio as the companion conversational agent.
- [ ] Use MCP tools to let Copilot Studio call custom backend capabilities.
- [ ] Use single-tenant Microsoft Entra ID authentication for the first pilot.
- [ ] Decide the first deployment environment name, Azure subscription, tenant, and resource group.
- [ ] Decide whether local development will use dev tunnels, ngrok, or another public callback URL strategy for Teams testing.
- [ ] Define the minimal retention policy for meeting data, transcript text, alerts, and recaps.

## 1. Repository And Project Setup

- [x] Create the solution structure.
- [x] Add an ASP.NET Core web app for the Teams side panel and host setup experience.
- [x] Add a backend API project or API area for meeting, agenda, transcript, and recap endpoints.
- [x] Add a shared domain/contracts project if the solution grows beyond one web project.
- [x] Add a test project for unit tests.
- [x] Add an integration test project for API and workflow tests.
- [x] Add a README with setup, local run, Teams testing, and configuration instructions.
- [x] Add `.gitignore` rules for .NET, Teams Toolkit artifacts, local secrets, build output, and IDE files.
- [x] Add basic CI steps for restore, build, test, and formatting checks.
- [x] Add app configuration files for local development and environment-specific settings.

## 2. Core Domain Model

- [x] Define `MeetingSession`.
- [x] Include meeting identity, organizer identity, title, scheduled start, scheduled end, status, and Teams meeting context identifiers.
- [x] Define `AgendaPlan`.
- [x] Include meeting id, objective, total duration, agenda sections, version, approval state, approved by, and approved timestamp.
- [x] Define `AgendaSection`.
- [x] Include title, purpose, suggested duration, order, and optional facilitation notes.
- [x] Define `FacilitatorAlert`.
- [x] Include meeting id, alert type, severity, timestamp, source, evidence snippet, recommendation, dismissed state, and resolved state.
- [x] Define `MeetingRecap`.
- [x] Include meeting id, pacing summary, topic drift moments, confusion moments, action items, and generated timestamp.
- [x] Define enums for meeting status, alert type, alert severity, alert source, transcript availability, and recap status.
- [x] Add validation rules for required fields, meeting duration bounds, agenda section durations, and host ownership.

## 3. Storage And Data Retention

- [ ] Choose and configure the Phase 1 database.
- [ ] Create persistence for meeting sessions.
- [ ] Create persistence for approved agenda plans and agenda versions.
- [ ] Create persistence for facilitator alerts.
- [ ] Create persistence for meeting recaps.
- [ ] Add a way to mark transcript-derived insights unavailable when transcription is disabled.
- [ ] Implement transcript text as transient processing input rather than durable long-term data.
- [ ] Add retention settings for raw transcript processing artifacts.
- [ ] Add retention settings for recap artifacts and meeting metadata.
- [ ] Add a background cleanup job for expired transcript artifacts and old meeting data.

## 4. Teams App Package

- [ ] Create the Teams app manifest.
- [ ] Configure the app as a Teams meeting app.
- [ ] Configure the meeting side panel capability.
- [ ] Configure the pre-meeting or meeting details setup surface if supported by the selected Teams app model.
- [ ] Configure app icons, name, description, and developer metadata.
- [ ] Configure valid domains for the hosted app.
- [ ] Configure required Teams and Graph permissions.
- [ ] Document the organizer installation flow.
- [ ] Document IT preinstall and tenant approval requirements.
- [ ] Test adding the app to a meeting before the meeting using `Add a tab (+)`.
- [ ] Test adding or opening the app during a meeting using `More > Add an app` or the meeting toolbar.
- [ ] Confirm the app opens as the intended meeting side panel.

## 5. Authentication And Authorization

- [ ] Register the app in Microsoft Entra ID.
- [ ] Configure single-tenant authentication.
- [ ] Configure redirect URIs for local development and hosted environments.
- [ ] Implement Teams SSO or the selected Teams authentication flow.
- [ ] Validate Teams meeting context tokens on backend calls.
- [ ] Enforce that only the organizer or authorized host can create, edit, or approve the meeting plan.
- [ ] Enforce that only authorized host users can view facilitator alerts and recaps.
- [ ] Add authorization checks to all meeting, agenda, alert, and recap APIs.
- [ ] Add audit logging for sensitive actions such as agenda approval and recap access.

## 6. Host Setup Experience

- [ ] Build the pre-meeting setup page or component.
- [ ] Capture meeting goal.
- [ ] Capture meeting duration.
- [ ] Capture optional host notes.
- [ ] Load meeting title and schedule from Teams context or Graph where available.
- [ ] Let the host generate a draft agenda.
- [ ] Show agenda sections with titles, purposes, and suggested durations.
- [ ] Let the host edit agenda section titles.
- [ ] Let the host edit agenda section durations.
- [ ] Let the host reorder agenda sections.
- [ ] Let the host add or remove agenda sections.
- [ ] Let the host approve the agenda.
- [ ] Save approved agenda versions.
- [ ] Show clear status when an agenda is draft, approved, or missing.

## 7. Agenda Generation

- [ ] Implement an agenda drafting service.
- [ ] Accept meeting goal, duration, optional notes, and optional meeting metadata.
- [ ] Generate a realistic agenda plan that fits inside the meeting duration.
- [ ] Include facilitation notes that help the host keep the meeting focused.
- [ ] Validate generated section durations before returning the plan.
- [ ] Add fallback behavior when the AI service fails.
- [ ] Add tests for short meetings, long meetings, vague goals, and overly broad goals.
- [ ] Add tests that generated section durations do not exceed total meeting duration.

## 8. In-Meeting Side Panel

- [ ] Build the Teams side panel view.
- [ ] Display meeting title and host-facing status.
- [ ] Display elapsed time and remaining time.
- [ ] Display the approved agenda.
- [ ] Highlight the currently expected agenda section based on elapsed time.
- [ ] Display pacing risk level.
- [ ] Display private facilitator alerts.
- [ ] Display suggested host interventions.
- [ ] Let the host dismiss alerts.
- [ ] Let the host mark alerts as acted on.
- [ ] Handle the missing-agenda state by prompting the host to create or approve a plan.
- [ ] Handle the meeting-not-started state.
- [ ] Handle the meeting-ended state.
- [ ] Keep the UI compact enough for the Teams side panel.

## 9. Live Pacing Engine

- [ ] Implement whole-meeting pacing calculations.
- [ ] Calculate elapsed time, remaining time, and percent complete.
- [ ] Compare elapsed time against agenda progress.
- [ ] Generate pacing alerts when the meeting is at risk of not finishing cleanly.
- [ ] Add default thresholds for mid-meeting, late-meeting, and endgame warnings.
- [ ] Generate suggested recovery actions such as skip, park, summarize, or move to decision.
- [ ] Avoid repeated duplicate alerts for the same pacing condition.
- [ ] Add tests for meetings that are on track, behind, nearly over, and past scheduled end time.

## 10. Copilot Studio Companion Agent

- [ ] Create the Copilot Studio agent.
- [ ] Define the facilitator persona as private, host-supportive, concise, and non-disruptive.
- [ ] Add topics or instructions for agenda drafting.
- [ ] Add topics or instructions for pacing guidance.
- [ ] Add topics or instructions for intervention suggestions.
- [ ] Add topics or instructions for post-meeting recap explanation.
- [ ] Publish the agent to Teams.
- [ ] Confirm the host can chat with the agent outside the meeting.
- [ ] Confirm whether the agent can be added to meeting chat under tenant policy.
- [ ] Document that the initial agent is not a participant that joins and listens to the call.

## 11. MCP Tooling

- [ ] Choose the MCP hosting approach.
- [ ] Implement an MCP server for facilitator backend tools.
- [ ] Expose `create_agenda_draft`.
- [ ] Expose `explain_pacing_risk`.
- [ ] Expose `suggest_host_intervention`.
- [ ] Expose `generate_host_recap`.
- [ ] Add authentication or access controls for MCP tool calls.
- [ ] Connect the MCP server to Copilot Studio.
- [ ] Add request and response schemas for every MCP tool.
- [ ] Add tests for valid tool calls, invalid inputs, unauthorized access, and backend failures.

## 12. Graph And Transcript Integration

- [ ] Register required Microsoft Graph permissions for meeting metadata and transcripts.
- [ ] Implement Graph client configuration.
- [ ] Fetch meeting metadata where allowed.
- [ ] Subscribe to or poll for post-meeting transcript availability.
- [ ] Implement the Graph transcript notification endpoint if using change notifications.
- [ ] Validate Graph webhook notifications.
- [ ] Handle duplicate transcript notifications idempotently.
- [ ] Fetch transcript content after it becomes available.
- [ ] Mark transcript insights unavailable when transcription is disabled or missing.
- [ ] Ensure raw transcript text is only retained according to the retention policy.
- [ ] Add tests for transcript available, transcript delayed, transcript missing, and permission failure cases.

## 13. Post-Meeting Intelligence

- [ ] Implement transcript processing jobs.
- [ ] Extract likely topic drift moments.
- [ ] Extract likely confusion or overload moments.
- [ ] Identify repeated clarification loops.
- [ ] Identify moments with dense or complex explanation.
- [ ] Generate concise evidence snippets for each insight.
- [ ] Generate host-facing recommendations for future facilitation.
- [ ] Generate action items where appropriate.
- [ ] Create the final `MeetingRecap`.
- [ ] Store the recap without storing raw transcript longer than necessary.
- [ ] Add tests for transcript analysis and recap generation.

## 14. Recap Experience

- [ ] Build the post-meeting recap view.
- [ ] Show pacing summary.
- [ ] Show topic drift moments.
- [ ] Show confusion or overload moments.
- [ ] Show action items.
- [ ] Show facilitation suggestions for the next meeting.
- [ ] Show when transcript-based insights are unavailable.
- [ ] Let the host access the recap from the Teams app.
- [ ] Let the companion agent answer questions about the recap through approved backend tools.

## 15. Privacy, Security, And Governance

- [ ] Document what data is stored.
- [ ] Document what data is transient.
- [ ] Document transcript handling.
- [ ] Document tenant admin permissions needed for Teams, Graph, and Copilot Studio.
- [ ] Add configurable retention settings.
- [ ] Add error handling that avoids exposing transcript or meeting content in logs.
- [ ] Add structured logging for operational events.
- [ ] Add health checks for the web app, database, queue, Graph integration, and AI/MCP services.
- [ ] Add secret management for local development and Azure deployment.
- [ ] Confirm compliance expectations for the internal pilot.

## 16. Pilot Deployment

- [ ] Create Azure hosting resources.
- [ ] Deploy the web app and backend APIs.
- [ ] Deploy the MCP server.
- [ ] Deploy database resources.
- [ ] Deploy queue or background worker resources.
- [ ] Configure environment variables and managed identity where possible.
- [ ] Upload or publish the Teams app package.
- [ ] Publish the Copilot Studio companion agent to Teams.
- [ ] Assign the app to pilot users.
- [ ] Run an end-to-end pilot meeting.
- [ ] Collect feedback from meeting hosts.
- [ ] Track whether hosts used the agenda, side panel, alerts, and recap.

## 17. Phase 1 Acceptance Criteria

- [ ] A host can install or access the Teams meeting app.
- [ ] A host can add the app to a Teams meeting before the meeting.
- [ ] A host can open the app as a Teams meeting side panel during the meeting.
- [ ] A host can enter a meeting goal and duration.
- [ ] The system can draft an agenda.
- [ ] The host can edit and approve the agenda.
- [ ] The side panel can show elapsed time, remaining time, agenda context, and pacing risk.
- [ ] The system can generate private host-facing pacing suggestions.
- [ ] The system can process a post-meeting transcript when available.
- [ ] The system can generate a brief host recap.
- [ ] The system handles missing or disabled transcription gracefully.
- [ ] The system avoids storing raw transcript text longer than the configured retention window.

## 18. Phase 2 TODO: Near-Real-Time Facilitation

- [ ] Validate the feasibility and approval requirements for Teams calling or media bot infrastructure.
- [ ] Design the real-time meeting ingestion service.
- [ ] Add Azure Speech or equivalent speech-to-text integration.
- [ ] Stream partial transcript windows into the backend.
- [ ] Add live topic drift scoring.
- [ ] Add live confusion and overload scoring.
- [ ] Add alert deduplication across rolling transcript windows.
- [ ] Add latency monitoring for live alerts.
- [ ] Add failure fallback to Phase 1 pacing-only behavior.
- [ ] Run real meeting tests for alert quality, timing, and host trust.

## 19. Phase 3 TODO: Meeting Operating System

- [ ] Add recurring meeting history.
- [ ] Add repeated overrun analytics.
- [ ] Add recurring confusion theme analytics.
- [ ] Add agenda quality feedback across meetings.
- [ ] Add team-specific facilitation preferences.
- [ ] Add admin-configurable policies.
- [ ] Add role-based views for hosts, presenters, and administrators.
- [ ] Add configurable analytics retention policies.
- [ ] Add reporting for meeting quality trends.

