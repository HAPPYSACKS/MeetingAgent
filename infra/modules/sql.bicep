targetScope = 'resourceGroup'

param location string
param tags object
param sqlServerName string
param sqlDatabaseName string
param tenantId string
param sqlAdminLogin string

@secure()
param sqlAdminPassword string

param sqlEntraAdminObjectId string
param sqlEntraAdminLogin string

resource sqlServer 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: sqlServerName
  location: location
  tags: tags
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
    administrators: {
      administratorType: 'ActiveDirectory'
      login: sqlEntraAdminLogin
      sid: sqlEntraAdminObjectId
      tenantId: tenantId
      azureADOnlyAuthentication: false
    }
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2025-02-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  tags: tags
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    freeLimitExhaustionBehavior: 'AutoPause'
    maxSizeBytes: 34359738368
    minCapacity: json('0.5')
    requestedBackupStorageRedundancy: 'Local'
    useFreeLimit: true
  }
}

resource databaseAuditing 'Microsoft.Sql/servers/databases/auditingSettings@2024-05-01-preview' = {
  parent: sqlDatabase
  name: 'default'
  properties: {
    state: 'Enabled'
    isAzureMonitorTargetEnabled: true
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output sqlDatabaseResourceId string = sqlDatabase.id
