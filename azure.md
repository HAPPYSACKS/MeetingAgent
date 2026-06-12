# MeetingAgent Azure Dev Environment

## Purpose

This document describes the Azure `dev` cloud environment for MeetingAgent.
It is both an infrastructure decision record and the source-of-truth summary for
the Bicep deployment under `infra/`.

The environment is designed for a Phase 1 internal development pilot: Teams-native,
host-private, transcript-first, and cost-conscious while using Azure credits.

## Environment Shape

- Environment name: `dev`
- Core Azure region: `canadacentral`
- Microsoft Foundry / Azure OpenAI region: `swedencentral`
- Resource group: `rg-meetingagent-dev`
- Infrastructure as Code: Bicep under `infra/`
- Deployment scope: subscription
- Teardown model: delete the dev resource group when the environment is no longer needed

Core app, storage, database, secrets, jobs, and monitoring resources should live in
Canada Central. Foundry/OpenAI should live in Sweden Central to improve access to
newer model features during development.

The current Bicep deployment creates the resource group, storage, Azure SQL, Key
Vault, Azure OpenAI account, Functions, monitoring, budget, role assignments, and
SQL free-limit alert. App Service hosts are implemented but disabled in the current
`dev` parameter file until the subscription has App Service plan quota in Canada
Central.

## Deployment Commands

Deploy:

```powershell
az deployment sub create `
  --location canadacentral `
  --template-file infra/main.bicep `
  --parameters infra/environments/dev.bicepparam
```

Destroy:

```powershell
az group delete --name rg-meetingagent-dev
```

The referenced Bicep files live under `infra/`. Keep those files aligned with this
document as the environment evolves.

## Resource Plan

| Capability | Resource | Region | SKU / Tier | Purpose |
| --- | --- | --- | --- | --- |
| Resource boundary | Resource group | `canadacentral` | N/A | Disposable environment container |
| Web hosting | Linux App Service Plan | `canadacentral` | Disabled in current `dev` params; `B1` Basic target; `F1` fallback params retained | Shared compute for web and MCP hosts |
| Teams web app and API | App Service | `canadacentral` | Conditional App Service app | Razor Pages, Teams side panel, setup, recap, APIs, Graph webhook endpoint |
| MCP host | App Service | `canadacentral` | Conditional App Service app | MCP tool server for Copilot Studio |
| Background jobs | Azure Functions | `canadacentral` | Flex Consumption `FC1`, .NET isolated 10.0 | Transcript processing, recap generation, retention cleanup, Graph notification follow-up |
| Storage and queues | StorageV2 account | `canadacentral` | `Standard_LRS`, hot | Function storage, queues, transient transcript artifacts |
| Product state | Azure SQL Database | `canadacentral` | `GP_S_Gen5_1`, free limit enabled | Meetings, agendas, alerts, recaps, retention metadata |
| Secrets | Key Vault | `canadacentral` | `Standard` | Secrets, external credentials, app integration settings |
| AI | Azure OpenAI account | `swedencentral` | `S0`, model deployments conditional | Agenda drafting, recap generation, transcript analysis |
| Observability | Log Analytics | `canadacentral` | 30-day retention | Central logs and telemetry storage |
| Observability | Application Insights | `canadacentral` | Workspace-based | App, MCP, and Functions telemetry |
| Cost control | Azure budget | Subscription | `$50/month` default | Alerts at 50%, 80%, and 100% |
| Cost control | Azure Monitor alert | `canadacentral` | N/A | Alert when Azure SQL free amount remaining is low |
| Managed identity access | Azure RBAC assignments | Resource group resources | Storage, Key Vault, Azure OpenAI data roles | Let app, MCP, and Functions use managed identity instead of secrets where possible |

## Resource Details

### App Service

- App Service deployment: conditional in Bicep and disabled in the current `dev` parameter file because the free-trial subscription currently has zero App Service plan quota in Canada Central.
- Temporary fallback considered: Linux `F1` Free, but the subscription still reports `Total VMs: 0` quota during App Service plan preflight validation.
- Target App Service Plan after quota is available: Linux `B1` Basic.
- Current disabled dev values: `F1`/`Free`, capacity `1`, Always On `false`.
- Web app name pattern: `app-meetingagent-web-dev-<suffix>`.
- MCP app name pattern: `app-meetingagent-mcp-dev-<suffix>`.
- Runtime: `.NET 10`.
- Linux runtime setting: `DOTNETCORE|10.0`.
- Both apps use system-assigned managed identities.
- HTTPS only, HTTP/2 enabled, FTPS disabled, minimum TLS 1.2.
- Always On: disabled on `F1`; enable it when moving back to `B1`.
- Deployment slots: omitted for dev.
- When disabled, the deployment outputs blank web/MCP URLs and principal ids, and
  `teamsValidDomains` is an empty array.

### Azure Functions

- Hosting plan: Flex Consumption.
- SKU: `FC1`.
- Runtime: .NET isolated 10.0.
- Always On: `false`.
- Maximum instance count: `40`.
- Instance memory: `2048` MB.
- Deployment package storage: `function-packages` blob container using system-assigned managed identity.
- Runtime storage access: managed identity-based `AzureWebJobsStorage` settings for blob, queue, and table endpoints.
- Jobs:
  - Transcript processing.
  - Recap generation.
  - Retention cleanup.
  - Graph notification follow-up.

### Storage

- Account kind: `StorageV2`.
- SKU: `Standard_LRS`.
- Access tier: hot.
- Public blob access: disabled.
- Shared key access: disabled.
- Minimum TLS: 1.2.
- HTTPS only: enabled.
- Public network access: enabled for dev.
- Network ACL default action: allow, with Azure services bypass.
- Blob and container soft delete retention: 7 days.
- Queues:
  - `transcript-processing`
  - `recap-generation`
  - `retention-cleanup`
  - `graph-notifications`
- Blob containers:
  - `transcript-artifacts`, private, lifecycle delete after 3 days.
  - `function-packages`, private, only if deployment packaging needs it.

### Azure SQL

- Logical server plus one database.
- Database SKU: `GP_S_Gen5_1`.
- Free limit: enabled.
- Free limit exhaustion behavior: `AutoPause`.
- Maximum size: 32 GB.
- Auto-pause delay: 60 minutes.
- Minimum capacity: 0.5 vCore.
- Backup storage redundancy: local.
- SQL Server public network access: enabled for dev.
- SQL auth: enabled as a bootstrap/admin path.
- Microsoft Entra administrator: configured, with Microsoft Entra-only authentication disabled for dev.
- Auditing: enabled with Azure Monitor as the target.

A post-deploy SQL step should create database users for the web app, MCP app, and
Functions managed identities. Bicep should create the server and database, but the
identity database grants are a follow-up configuration step.

### Key Vault

- SKU: `Standard`.
- RBAC authorization: enabled.
- Soft delete retention: 7 days.
- Purge protection: disabled for dev teardown flexibility.
- Public network access: enabled for dev.
- Network ACL default action: allow, with Azure services bypass.
- Store Graph client credentials only if Graph cannot be handled with managed
  identity or app role assignment.

### Microsoft Foundry / Azure OpenAI

- Resource location: `swedencentral`.
- Resource type: `Microsoft.CognitiveServices/accounts` with `kind: OpenAI`.
- SKU: `S0`.
- Local key authentication: disabled.
- Public network access: enabled for dev.
- Model deployments: disabled by default in `dev` until the subscription has quota.
- Planned primary deployment:
  - Model: `gpt-chat-latest`.
  - Version: `2026-05-28`.
  - Deployment SKU: `GlobalStandard`.
- Planned secondary deployment:
  - Model: `gpt-5.5`.
  - Version: `2026-04-24`.
  - Deployment SKU: `Standard`.

After model quota is available, the app should default to the primary deployment
for development experimentation and allow switching to the secondary deployment
through configuration.

If quota blocks either deployment, rerun with that deployment disabled. The rest of
the environment should remain deployable.

### Managed Identity And RBAC

- Web app, MCP app, and Functions use system-assigned managed identities when the
  corresponding hosts are deployed.
- The storage account grants those identities:
  - `Storage Blob Data Contributor`
  - `Storage Queue Data Contributor`
- Key Vault grants those identities:
  - `Key Vault Secrets User`
- Azure OpenAI grants those identities:
  - `Cognitive Services OpenAI User`
- Azure SQL database users and grants are intentionally post-deploy steps because
  they are database-level operations.

When App Service is disabled, only the Functions managed identity receives these
data-plane role assignments.

## Feature-To-Resource Map

- Teams setup, side panel, and recap views: web App Service.
- Backend APIs for meeting, agenda, alert, and recap workflows: web App Service.
- Copilot Studio MCP tools: MCP App Service.
- Agenda drafting and recap generation: Foundry/OpenAI in Sweden Central.
- Persistent meeting state: Azure SQL Database.
- Post-meeting transcript processing: Azure Functions, Storage Queues, and transient Blob Storage.
- Graph transcript notifications: public webhook endpoint on web app, queued to Functions.
- Retention cleanup: timer-triggered Function.
- Secrets and external credentials: Key Vault.
- Health checks, traces, logs, and failures: Application Insights and Log Analytics.
- Credit protection: Azure budget and Azure SQL free-limit alert.

## Implemented Bicep Layout

```text
infra/
  main.bicep
  environments/
    dev.bicepparam
  modules/
    appservice.bicep
    functions.bicep
    storage.bicep
    sql.bicep
    keyvault.bicep
    ai.bicep
    monitoring.bicep
    budget.bicep
    roleAssignments.bicep
    aiRoleAssignments.bicep
    sql-free-limit-alert.bicep
```

## Important Parameters

```text
environmentName = dev
location = canadacentral
aiLocation = swedencentral
namePrefix = meetingagent
deployAppService = false
appServicePlanSkuName = F1
appServicePlanSkuTier = Free
appServicePlanSkuSize = F1
appServicePlanSkuFamily = F
appServicePlanCapacity = 1
appServiceAlwaysOn = false
tenantId
sqlAdminLogin
sqlAdminPassword secure
sqlEntraAdminObjectId
sqlEntraAdminLogin
budgetContactEmails
monthlyBudgetAmountUsd = 50
deployAi = true
deployLatestChatModel = false
deployReasoningModel = false
aiPrimaryDeploymentName = gpt-chat-latest
aiPrimaryModelName = gpt-chat-latest
aiPrimaryModelVersion = 2026-05-28
aiPrimaryDeploymentSku = GlobalStandard
aiSecondaryDeploymentName = gpt-5-5
aiSecondaryModelName = gpt-5.5
aiSecondaryModelVersion = 2026-04-24
aiSecondaryDeploymentSku = Standard
sqlFreeAmountRemainingThreshold = 10000
```

## Expected Outputs

```text
resourceGroupName
webAppUrl
mcpAppUrl
webAppPrincipalId
mcpAppPrincipalId
functionsPrincipalId
sqlServerName
sqlDatabaseName
storageAccountName
keyVaultName
aiEndpoint
aiPrimaryDeploymentName
aiSecondaryDeploymentName
teamsValidDomains
```

## App Settings

Shared app settings passed to App Service hosts and Functions:

```text
ASPNETCORE_ENVIRONMENT=Development
APPLICATIONINSIGHTS_CONNECTION_STRING
MeetingAgent__Retention__TranscriptArtifactDays=3
MeetingAgent__Retention__RecapArtifactDays=30
MeetingAgent__Retention__MeetingMetadataDays=180
MeetingAgent__RetentionCleanup__Enabled=true
MeetingAgent__RetentionCleanup__IntervalMinutes=1440
Storage__AccountName
Storage__TranscriptQueueName
Storage__RecapQueueName
Storage__GraphNotificationsQueueName
Sql__ServerName
Sql__DatabaseName
Sql__UseManagedIdentity=true
Sql__CommandTimeoutSeconds=30
AzureOpenAI__Endpoint
AzureOpenAI__DeploymentName
AzureOpenAI__SecondaryDeploymentName
AzureAd__TenantId
Teams__ValidDomains
```

Additional Function App settings are added by `functions.bicep`:

```text
AzureWebJobsStorage__credential=managedidentity
AzureWebJobsStorage__blobServiceUri
AzureWebJobsStorage__queueServiceUri
AzureWebJobsStorage__tableServiceUri
FUNCTIONS_EXTENSION_VERSION=~4
```

## Deployment Validation Checklist

- Confirm Canada Central supports selected App Service, Functions Flex Consumption, SQL, Storage, and Key Vault SKUs for the subscription.
- Confirm Sweden Central supports the selected Foundry model deployments for the subscription and has quota.
- Confirm `.NET 10` appears in App Service Linux runtimes before deploying the apps.
- If `deployAppService=true`, confirm `GET /health` succeeds on the web app.
- If `deployAppService=true`, confirm `GET /health` succeeds on the MCP app.
- If `deployAppService=true`, confirm `GET /tools` on the MCP app returns the facilitator tool catalog.
- Confirm the Function App can read and write configured queues.
- Confirm managed identities can read allowed Key Vault secrets.
- Confirm Functions can connect to Azure SQL after post-deploy grants.
- If `deployAppService=true`, confirm web and MCP can connect to Azure SQL after post-deploy grants.
- Confirm Azure OpenAI account deployment succeeds.
- If model deployments are enabled, confirm Azure OpenAI test calls succeed against the primary deployment.
- Confirm Application Insights receives request and dependency telemetry.
- Confirm budget and Azure SQL free-limit alerts are present.

## Excluded From Phase 1 Dev

- Azure Kubernetes Service.
- Container Apps.
- API Management.
- Front Door.
- Service Bus.
- Azure SignalR.
- Azure Speech.
- Teams media or calling bot infrastructure.
- Private endpoints and VNet integration.

## Assumptions

- App registrations, Teams app package publication, Graph permissions, and Copilot Studio publishing are adjacent Microsoft 365 setup steps, not Azure ARM resources.
- Public endpoints are acceptable for dev because Teams and Copilot Studio must reach the apps.
- Azure SQL free offer is used to protect the credit.
- Foundry is intentionally outside Canada Central for feature access; meeting metadata and app storage remain in Canada Central.
- Azure OpenAI model availability and quota can change, so the AI module should be conditional and parameterized.

## References

- Azure SQL free offer: <https://learn.microsoft.com/en-us/azure/azure-sql/database/free-offer>
- Azure Functions hosting: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-scale>
- Foundry model region availability: <https://learn.microsoft.com/en-us/azure/foundry/foundry-models/concepts/models-sold-directly-by-azure-region-availability>
- Copilot Studio MCP connection: <https://learn.microsoft.com/en-us/microsoft-copilot-studio/mcp-add-existing-server-to-agent>
