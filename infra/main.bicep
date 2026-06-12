targetScope = 'subscription'

@description('Environment name used for resource naming and tags.')
param environmentName string = 'dev'

@description('Primary Azure region for core resources.')
param location string = 'canadacentral'

@description('Azure region for Microsoft Foundry / Azure OpenAI resources.')
param aiLocation string = 'swedencentral'

@description('Short project prefix used in resource names.')
param namePrefix string = 'meetingagent'

@description('Deploy the Teams web and MCP App Service hosts.')
param deployAppService bool = true

@description('App Service plan SKU name for the Teams web and MCP hosts.')
param appServicePlanSkuName string = 'B1'

@description('App Service plan SKU tier for the Teams web and MCP hosts.')
param appServicePlanSkuTier string = 'Basic'

@description('App Service plan SKU size for the Teams web and MCP hosts.')
param appServicePlanSkuSize string = 'B1'

@description('App Service plan SKU family for the Teams web and MCP hosts.')
param appServicePlanSkuFamily string = 'B'

@description('App Service plan instance count for the Teams web and MCP hosts.')
param appServicePlanCapacity int = 1

@description('Enable Always On for the Teams web and MCP hosts. Free/shared plans do not support Always On.')
param appServiceAlwaysOn bool = true

@description('Microsoft Entra tenant id for Azure SQL and app settings.')
param tenantId string = tenant().tenantId

@description('Azure SQL administrator login. SQL auth is provided as a bootstrap path; app access uses managed identities after post-deploy grants.')
param sqlAdminLogin string

@secure()
@description('Azure SQL administrator password.')
param sqlAdminPassword string

@description('Microsoft Entra object id for the Azure SQL server administrator.')
param sqlEntraAdminObjectId string

@description('Microsoft Entra display/login name for the Azure SQL server administrator.')
param sqlEntraAdminLogin string

@description('Email addresses that receive budget and Azure Monitor alert notifications.')
param budgetContactEmails array

@description('Monthly subscription budget amount for the dev environment.')
param monthlyBudgetAmountUsd int = 50

@description('Deploy the Azure AI Foundry / Azure OpenAI account and model deployments.')
param deployAi bool = true

@description('Deploy the primary latest chat model.')
param deployLatestChatModel bool = true

@description('Deploy the secondary reasoning model.')
param deployReasoningModel bool = true

@description('Primary Azure OpenAI model deployment name.')
param aiPrimaryDeploymentName string = 'gpt-chat-latest'

@description('Primary Azure OpenAI model name.')
param aiPrimaryModelName string = 'gpt-chat-latest'

@description('Primary Azure OpenAI model version.')
param aiPrimaryModelVersion string = '2026-05-28'

@description('Primary Azure OpenAI deployment SKU.')
param aiPrimaryDeploymentSku string = 'GlobalStandard'

@description('Secondary Azure OpenAI model deployment name.')
param aiSecondaryDeploymentName string = 'gpt-5-5'

@description('Secondary Azure OpenAI model name.')
param aiSecondaryModelName string = 'gpt-5.5'

@description('Secondary Azure OpenAI model version.')
param aiSecondaryModelVersion string = '2026-04-24'

@description('Secondary Azure OpenAI deployment SKU.')
param aiSecondaryDeploymentSku string = 'Standard'

@description('Optional SQL free-offer alert threshold in vCore seconds remaining.')
param sqlFreeAmountRemainingThreshold int = 10000

var normalizedPrefix = toLower(replace(namePrefix, '-', ''))
var uniqueSuffix = toLower(uniqueString(subscription().id, environmentName, location))
var tags = {
  app: 'MeetingAgent'
  environment: environmentName
  costOwner: 'dev'
  retention: 'minimal'
}

var resourceGroupName = 'rg-${namePrefix}-${environmentName}'
var appPlanName = 'plan-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}'
var webAppName = 'app-${namePrefix}-web-${environmentName}-${take(uniqueSuffix, 6)}'
var mcpAppName = 'app-${namePrefix}-mcp-${environmentName}-${take(uniqueSuffix, 6)}'
var functionPlanName = 'plan-${namePrefix}-func-${environmentName}-${take(uniqueSuffix, 6)}'
var functionAppName = 'func-${namePrefix}-jobs-${environmentName}-${take(uniqueSuffix, 6)}'
var storageAccountName = take('st${normalizedPrefix}${uniqueSuffix}', 24)
var sqlServerName = 'sql-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}'
var sqlDatabaseName = 'sqldb-${namePrefix}-${environmentName}'
var keyVaultName = take('kv-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}', 24)
var logAnalyticsName = 'law-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}'
var appInsightsName = 'appi-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}'
var actionGroupName = 'ag-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}'
var aiAccountName = 'aoai-${namePrefix}-${environmentName}-${take(uniqueSuffix, 6)}'

resource rg 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module monitoring './modules/monitoring.bicep' = {
  name: 'monitoring-${environmentName}'
  scope: rg
  params: {
    location: location
    tags: tags
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
    actionGroupName: actionGroupName
    alertEmails: budgetContactEmails
  }
}

module storage './modules/storage.bicep' = {
  name: 'storage-${environmentName}'
  scope: rg
  params: {
    location: location
    tags: tags
    storageAccountName: storageAccountName
  }
}

module sql './modules/sql.bicep' = {
  name: 'sql-${environmentName}'
  scope: rg
  params: {
    location: location
    tags: tags
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
    tenantId: tenantId
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    sqlEntraAdminObjectId: sqlEntraAdminObjectId
    sqlEntraAdminLogin: sqlEntraAdminLogin
  }
}

module keyVault './modules/keyvault.bicep' = {
  name: 'keyvault-${environmentName}'
  scope: rg
  params: {
    location: location
    tags: tags
    keyVaultName: keyVaultName
    tenantId: tenantId
  }
}

var aiDeployments = [
  {
    deploy: deployAi && deployLatestChatModel
    deploymentName: aiPrimaryDeploymentName
    modelName: aiPrimaryModelName
    modelVersion: aiPrimaryModelVersion
    skuName: aiPrimaryDeploymentSku
    capacity: 1
  }
  {
    deploy: deployAi && deployReasoningModel
    deploymentName: aiSecondaryDeploymentName
    modelName: aiSecondaryModelName
    modelVersion: aiSecondaryModelVersion
    skuName: aiSecondaryDeploymentSku
    capacity: 1
  }
]

module ai './modules/ai.bicep' = if (deployAi) {
  name: 'ai-${environmentName}'
  scope: rg
  params: {
    location: aiLocation
    tags: tags
    accountName: aiAccountName
    deployments: aiDeployments
  }
}

var appSettings = {
  ASPNETCORE_ENVIRONMENT: 'Development'
  APPLICATIONINSIGHTS_CONNECTION_STRING: monitoring.outputs.applicationInsightsConnectionString
  MeetingAgent__Retention__TranscriptArtifactDays: '3'
  MeetingAgent__Retention__RecapArtifactDays: '30'
  MeetingAgent__Retention__MeetingMetadataDays: '180'
  Storage__AccountName: storage.outputs.storageAccountName
  Storage__TranscriptQueueName: 'transcript-processing'
  Storage__RecapQueueName: 'recap-generation'
  Storage__GraphNotificationsQueueName: 'graph-notifications'
  Sql__ServerName: sql.outputs.sqlServerFullyQualifiedDomainName
  Sql__DatabaseName: sql.outputs.sqlDatabaseName
  AzureOpenAI__Endpoint: ai.?outputs.endpoint ?? ''
  AzureOpenAI__DeploymentName: deployAi && deployLatestChatModel ? aiPrimaryDeploymentName : ''
  AzureOpenAI__SecondaryDeploymentName: deployAi && deployReasoningModel ? aiSecondaryDeploymentName : ''
  AzureAd__TenantId: tenantId
  Teams__ValidDomains: '${webAppName}.azurewebsites.net;${mcpAppName}.azurewebsites.net'
}

module appservice './modules/appservice.bicep' = if (deployAppService) {
  name: 'appservice-${environmentName}'
  scope: rg
  params: {
    location: location
    tags: tags
    appServicePlanName: appPlanName
    webAppName: webAppName
    mcpAppName: mcpAppName
    appServicePlanSkuName: appServicePlanSkuName
    appServicePlanSkuTier: appServicePlanSkuTier
    appServicePlanSkuSize: appServicePlanSkuSize
    appServicePlanSkuFamily: appServicePlanSkuFamily
    appServicePlanCapacity: appServicePlanCapacity
    appServiceAlwaysOn: appServiceAlwaysOn
    appSettings: appSettings
  }
}

var appServicePrincipalIds = deployAppService ? [
  appservice!.outputs.webAppPrincipalId
  appservice!.outputs.mcpAppPrincipalId
] : []

module functions './modules/functions.bicep' = {
  name: 'functions-${environmentName}'
  scope: rg
  params: {
    location: location
    tags: tags
    functionPlanName: functionPlanName
    functionAppName: functionAppName
    storageAccountName: storage.outputs.storageAccountName
    functionPackagesContainerName: 'function-packages'
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
    appSettings: appSettings
  }
}

module roleAssignments './modules/roleAssignments.bicep' = {
  name: 'roleAssignments-${environmentName}'
  scope: rg
  params: {
    storageAccountName: storage.outputs.storageAccountName
    keyVaultName: keyVault.outputs.keyVaultName
    principalIds: concat(appServicePrincipalIds, [
      functions.outputs.functionAppPrincipalId
    ])
  }
}

module aiRoleAssignments './modules/aiRoleAssignments.bicep' = if (deployAi) {
  name: 'aiRoleAssignments-${environmentName}'
  scope: rg
  params: {
    aiAccountName: ai.?outputs.accountName ?? ''
    principalIds: concat(appServicePrincipalIds, [
      functions.outputs.functionAppPrincipalId
    ])
  }
}

module sqlFreeLimitAlert './modules/sql-free-limit-alert.bicep' = {
  name: 'sql-free-limit-alert-${environmentName}'
  scope: rg
  params: {
    tags: tags
    alertName: 'alert-${namePrefix}-sql-free-remaining-${environmentName}'
    sqlDatabaseResourceId: sql.outputs.sqlDatabaseResourceId
    actionGroupResourceId: monitoring.outputs.actionGroupResourceId
    threshold: sqlFreeAmountRemainingThreshold
  }
}

module budget './modules/budget.bicep' = {
  name: 'budget-${environmentName}'
  params: {
    environmentName: environmentName
    amount: monthlyBudgetAmountUsd
    contactEmails: budgetContactEmails
  }
}

output resourceGroupName string = rg.name
output webAppUrl string = appservice.?outputs.webAppUrl ?? ''
output mcpAppUrl string = appservice.?outputs.mcpAppUrl ?? ''
output webAppPrincipalId string = appservice.?outputs.webAppPrincipalId ?? ''
output mcpAppPrincipalId string = appservice.?outputs.mcpAppPrincipalId ?? ''
output functionsPrincipalId string = functions.outputs.functionAppPrincipalId
output sqlServerName string = sql.outputs.sqlServerName
output sqlDatabaseName string = sql.outputs.sqlDatabaseName
output storageAccountName string = storage.outputs.storageAccountName
output keyVaultName string = keyVault.outputs.keyVaultName
output aiEndpoint string = ai.?outputs.endpoint ?? ''
output aiPrimaryDeploymentName string = deployAi && deployLatestChatModel ? aiPrimaryDeploymentName : ''
output aiSecondaryDeploymentName string = deployAi && deployReasoningModel ? aiSecondaryDeploymentName : ''
output teamsValidDomains array = deployAppService ? [
  '${webAppName}.azurewebsites.net'
  '${mcpAppName}.azurewebsites.net'
] : []
