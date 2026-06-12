using '../main.bicep'

param environmentName = 'dev'
param location = 'canadacentral'
param aiLocation = 'swedencentral'
param namePrefix = 'meetingagent'
param deployAppService = false
param appServicePlanSkuName = 'F1'
param appServicePlanSkuTier = 'Free'
param appServicePlanSkuSize = 'F1'
param appServicePlanSkuFamily = 'F'
param appServicePlanCapacity = 1
param appServiceAlwaysOn = false
param tenantId = readEnvironmentVariable('AZURE_TENANT_ID')
param sqlAdminLogin = 'meetingagentadmin'
param sqlAdminPassword = readEnvironmentVariable('MEETINGAGENT_SQL_ADMIN_PASSWORD')
param sqlEntraAdminObjectId = readEnvironmentVariable('MEETINGAGENT_SQL_ENTRA_ADMIN_OBJECT_ID')
param sqlEntraAdminLogin = readEnvironmentVariable('MEETINGAGENT_SQL_ENTRA_ADMIN_LOGIN')
param budgetContactEmails = [
  readEnvironmentVariable('MEETINGAGENT_BUDGET_EMAIL')
]
param monthlyBudgetAmountUsd = 50
param deployAi = true
param deployLatestChatModel = false
param deployReasoningModel = false
param aiPrimaryDeploymentName = 'gpt-chat-latest'
param aiPrimaryModelName = 'gpt-chat-latest'
param aiPrimaryModelVersion = '2026-05-28'
param aiPrimaryDeploymentSku = 'GlobalStandard'
param aiSecondaryDeploymentName = 'gpt-5-5'
param aiSecondaryModelName = 'gpt-5.5'
param aiSecondaryModelVersion = '2026-04-24'
param aiSecondaryDeploymentSku = 'Standard'
