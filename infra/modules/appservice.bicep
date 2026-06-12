targetScope = 'resourceGroup'

param location string
param tags object
param appServicePlanName string
param webAppName string
param mcpAppName string
param appServicePlanSkuName string
param appServicePlanSkuTier string
param appServicePlanSkuSize string
param appServicePlanSkuFamily string
param appServicePlanCapacity int
param appServiceAlwaysOn bool
param appSettings object

var appSettingsArray = [for setting in items(appSettings): {
  name: setting.key
  value: setting.value
}]

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: appServicePlanSkuName
    tier: appServicePlanSkuTier
    size: appServicePlanSkuSize
    family: appServicePlanSkuFamily
    capacity: appServicePlanCapacity
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  tags: union(tags, {
    role: 'teams-web'
  })
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      alwaysOn: appServiceAlwaysOn
      ftpsState: 'Disabled'
      http20Enabled: true
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.2'
      appSettings: appSettingsArray
    }
  }
}

resource mcpApp 'Microsoft.Web/sites@2024-04-01' = {
  name: mcpAppName
  location: location
  tags: union(tags, {
    role: 'mcp-host'
  })
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      alwaysOn: appServiceAlwaysOn
      ftpsState: 'Disabled'
      http20Enabled: true
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.2'
      appSettings: appSettingsArray
    }
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppPrincipalId string = webApp.identity.principalId
output mcpAppName string = mcpApp.name
output mcpAppUrl string = 'https://${mcpApp.properties.defaultHostName}'
output mcpAppPrincipalId string = mcpApp.identity.principalId
