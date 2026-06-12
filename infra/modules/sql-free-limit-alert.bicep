targetScope = 'resourceGroup'

param tags object
param alertName string
param sqlDatabaseResourceId string
param actionGroupResourceId string
param threshold int = 10000

resource alert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: alertName
  location: 'global'
  tags: tags
  properties: {
    description: 'Alerts when Azure SQL free database vCore seconds remaining falls below the configured threshold.'
    enabled: true
    severity: 2
    scopes: [
      sqlDatabaseResourceId
    ]
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'FreeAmountRemainingLow'
          metricNamespace: 'Microsoft.Sql/servers/databases'
          metricName: 'free_amount_remaining'
          operator: 'LessThan'
          threshold: threshold
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroupResourceId
      }
    ]
  }
}

output alertResourceId string = alert.id
