targetScope = 'resourceGroup'

param aiAccountName string
param principalIds array

var cognitiveServicesOpenAiUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd')

resource aiAccount 'Microsoft.CognitiveServices/accounts@2026-03-15-preview' existing = {
  name: aiAccountName
}

resource aiAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  name: guid(aiAccount.id, principalId, cognitiveServicesOpenAiUserRoleId)
  scope: aiAccount
  properties: {
    roleDefinitionId: cognitiveServicesOpenAiUserRoleId
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
