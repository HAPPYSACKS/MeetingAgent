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

Before running `zip.ps1`, define a local `.env` file. The file is ignored by git,
so it can hold machine- or tenant-specific package values such as the active dev
tunnel URL.

```powershell
Copy-Item .env.example .env
notepad .env
```

Minimum `.env` contents:

```text
MEETINGAGENT_TEAMS_BASE_URL=https://<your-dev-tunnel-host>
```

Optionally set `MEETINGAGENT_TEAMS_APP_ID` if you want to reuse the same Teams
app id across package rebuilds.

For a plain Teams package without SSO metadata, keep:

```text
MEETINGAGENT_INCLUDE_PREVIEW_AUTH=false
```

For Teams SSO testing after the Entra app registration is configured, set:

```text
MEETINGAGENT_INCLUDE_PREVIEW_AUTH=true
MEETINGAGENT_ENTRA_CLIENT_ID=<entra-application-client-id>
```

Generate the Teams app package:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\zip.ps1
```

The script creates `artifacts/teams/MeetingAgent.TeamsApp.zip`.

The rendered manifest uses:

- `meetingDetailsTab` for pre-meeting configuration from meeting details.
- `meetingSidePanel` for the in-meeting host surface.
- `groupChat` scope for scheduled private meetings.

The script can include preview authentication metadata after the matching
Microsoft Entra app registration is fully configured:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\New-TeamsAppPackage.ps1 `
  -BaseUrl "https://<your-dev-tunnel-host>" `
  -TeamsAppId "<new-guid-for-the-teams-app>" `
  -EntraClientId "<entra-application-client-id>" `
  -IncludePreviewAuth
```

Meeting-specific resource-specific consent permissions for Graph are not needed
for basic Teams tab SSO testing. Add `-IncludeRscPermissions` only when Graph
meeting metadata or transcript retrieval is being tested and tenant consent is
ready.

## Microsoft Entra App Registration

Create the app registration as a single-tenant app in the pilot tenant. This repo does not create or update the tenant registration automatically.

Use these registration values:

- Supported account types: accounts in this organizational directory only.
- Access token version: `requestedAccessTokenVersion = 2`.
- Application ID URI: `api://<lowercase-dev-tunnel-or-host>/<client-id>`.
- Exposed API scope: `access_as_user`.
- Authentication platform: Web.
- Enable implicit grant ID tokens for the fallback browser sign-in flow.
- Preauthorized client applications:
  - Teams desktop/mobile: `1fec8e78-bce4-4aaf-ab1b-5451cc387264`
  - Teams web: `5e3ce6c0-2b1f-4285-8d4b-75ee78787346`
- Web redirect URIs:
  - `https://localhost:7179/signin-oidc`
  - `https://<your-dev-tunnel-host>/signin-oidc`
  - `https://<hosted-web-app-host>/signin-oidc`
- Front-channel logout URL, if configured: `https://<host>/signout-callback-oidc`.

Local app settings can be provided with user secrets or environment variables:

```text
AzureAd__TenantId=<tenant-id>
AzureAd__ClientId=<client-id>
AzureAd__Domain=<tenant-domain>
Teams__ApplicationIdUri=api://<lowercase-dev-tunnel-or-host>/<client-id>
Teams__AllowedTenantId=<tenant-id>
Teams__ValidDomains__0=<lowercase-dev-tunnel-or-host>
```

When the registration is ready, build the Teams package with preview auth metadata:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\New-TeamsAppPackage.ps1 `
  -BaseUrl "https://<your-dev-tunnel-host>" `
  -TeamsAppId "<teams-app-id>" `
  -EntraClientId "<client-id>" `
  -IncludePreviewAuth
```

The Teams tab should call backend APIs with the token from `microsoftTeams.authentication.getAuthToken()`. The backend treats Teams context values as untrusted selectors and authorizes access from the Entra token identity plus stored meeting ownership.

The current dev SSO path has been verified with the Teams meeting app opening the
host setup page and completing the fallback Entra login flow. Resource-specific
Graph permissions remain separated behind `-IncludeRscPermissions` and should be
enabled only when Graph meeting metadata or transcript retrieval is ready to test.

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

Graph transcript retrieval is tracked separately from this package slice.
