targetScope = 'resourceGroup'

param location string
param tags object
param logAnalyticsName string
param appInsightsName string
param actionGroupName string
param alertEmails array

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
  }
}

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: actionGroupName
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'mtgadev'
    enabled: true
    emailReceivers: [for (email, index) in alertEmails: {
      name: 'email-${index}'
      emailAddress: email
      useCommonAlertSchema: true
    }]
  }
}

output logAnalyticsWorkspaceId string = workspace.id
output applicationInsightsName string = appInsights.name
output applicationInsightsConnectionString string = appInsights.properties.ConnectionString
output actionGroupResourceId string = actionGroup.id
