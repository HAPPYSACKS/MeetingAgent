# Teams App Package

This directory contains the Teams meeting app package source for the Phase 1 private-meeting pilot.

The package is designed for local Teams testing through a Microsoft dev tunnel that exposes `src/MeetingAgent.Web` on HTTPS port `7179`.

## Dev Tunnel Setup

Install the dev tunnel CLI:

```powershell
winget install Microsoft.devtunnel
```

Sign in and create an anonymous HTTPS tunnel for Teams:

```powershell
devtunnel user login
devtunnel create meetingagent-web --allow-anonymous --expiration 30d
devtunnel port create meetingagent-web -p 7179 --protocol https
devtunnel host meetingagent-web
```

Keep the tunnel command running and copy the public HTTPS URL it prints.

In another terminal, run the web app:

```powershell
dotnet run --project src/MeetingAgent.Web --launch-profile https
```

## Build The App Package

Create or reuse a single-tenant Microsoft Entra app registration for development, then generate the Teams app package:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\New-TeamsAppPackage.ps1 `
  -BaseUrl "https://<your-dev-tunnel-host>" `
  -TeamsAppId "<new-guid-for-the-teams-app>" `
  -EntraClientId "<entra-application-client-id>"
```

The script creates `artifacts/teams/MeetingAgent.TeamsApp.zip`.

The rendered manifest uses:

- `meetingDetailsTab` for pre-meeting configuration from meeting details.
- `meetingSidePanel` for the in-meeting host surface.
- `groupchat` scope for scheduled private meetings.
- `OnlineMeeting.ReadBasic.Chat` and `OnlineMeetingTranscript.Read.Chat` as meeting-specific RSC application permissions.

## Organizer Install Flow

1. Upload `artifacts/teams/MeetingAgent.TeamsApp.zip` through Teams Developer Portal or custom app upload.
2. Open a scheduled private Teams meeting.
3. Before the meeting, select `Add a tab (+)` and choose `MeetingAgent`.
4. Save the tab configuration.
5. During the meeting, open the app from the meeting apps list, meeting toolbar, or `More > Add an app`.
6. Confirm the side panel loads the MeetingAgent host setup page through the dev tunnel.

## IT Preinstall And Tenant Approval

Tenant administrators may need to:

- Enable custom app upload for pilot users.
- Approve the app in Teams admin center.
- Allow or pin the app through a Teams app setup policy.
- Review the resource-specific consent permissions in the manifest.
- Approve the matching Entra app registration used by `webApplicationInfo`.

Full Teams SSO, redirect URI setup, token validation, organizer authorization, and Graph transcript retrieval are tracked separately from this package slice.
