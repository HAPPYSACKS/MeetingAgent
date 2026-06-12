# MeetingAgent Azure Infrastructure

This folder contains the planned Bicep deployment for the `dev` cloud environment described in [`../azure.md`](../azure.md).

## Required Environment Variables

The dev parameter file reads sensitive and tenant-specific values from environment variables:

```powershell
$env:AZURE_TENANT_ID = '<tenant-guid>'
$env:MEETINGAGENT_SQL_ADMIN_PASSWORD = '<strong-password>'
$env:MEETINGAGENT_SQL_ENTRA_ADMIN_OBJECT_ID = '<user-or-group-object-id>'
$env:MEETINGAGENT_SQL_ENTRA_ADMIN_LOGIN = '<user-or-group-display-name>'
$env:MEETINGAGENT_BUDGET_EMAIL = '<alert-email>'
```

## Deploy

```powershell
az deployment sub create `
  --location canadacentral `
  --template-file infra/main.bicep `
  --parameters infra/environments/dev.bicepparam
```

## Destroy

```powershell
az group delete --name rg-meetingagent-dev
```

## Post-Deploy Tasks

- Create Azure SQL database users for the web app, MCP app, and Functions managed identities.
- Register/configure Microsoft Entra app registrations for Teams SSO, Graph, and Copilot Studio as needed.
- Update the Teams manifest `validDomains` with the deployment outputs.
- Connect Copilot Studio to the MCP app URL from the deployment outputs.
