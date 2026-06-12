targetScope = 'resourceGroup'

param location string
param tags object
param accountName string

@description('Model deployment descriptors. Entries with deploy=false are skipped.')
param deployments array

resource account 'Microsoft.CognitiveServices/accounts@2026-03-15-preview' = {
  name: accountName
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: accountName
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    disableLocalAuth: true
  }
}

@batchSize(1)
resource modelDeployments 'Microsoft.CognitiveServices/accounts/deployments@2026-03-15-preview' = [for deployment in deployments: if (deployment.deploy) {
  parent: account
  name: deployment.deploymentName
  sku: {
    name: deployment.skuName
    capacity: deployment.capacity
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: deployment.modelName
      version: deployment.modelVersion
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
  }
}]

output accountName string = account.name
output accountResourceId string = account.id
output endpoint string = account.properties.endpoint
