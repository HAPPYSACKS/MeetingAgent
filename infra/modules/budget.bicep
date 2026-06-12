targetScope = 'subscription'

param environmentName string
param amount int
param contactEmails array

@description('Budget start date. Defaults to the first day of the current UTC month.')
param startDate string = utcNow('yyyy-MM-01T00:00:00Z')

@description('Budget end date far enough out for recurring dev usage.')
param endDate string = '2036-12-31T00:00:00Z'

resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: 'budget-meetingagent-${environmentName}'
  properties: {
    category: 'Cost'
    amount: amount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: startDate
      endDate: endDate
    }
    notifications: {
      Actual_GreaterThan_50_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 50
        contactEmails: contactEmails
      }
      Actual_GreaterThan_80_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 80
        contactEmails: contactEmails
      }
      Actual_GreaterThan_100_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        contactEmails: contactEmails
      }
    }
  }
}

output budgetName string = budget.name
