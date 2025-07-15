@description('APIM service name')
param apimServiceName string

@description('Application Insights resource ID')
param applicationInsightsId string

@description('Application Insights instrumentation key')
param applicationInsightsInstrumentationKey string

// Reference existing APIM service
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' existing = {
  name: apimServiceName
}

// Application Insights Logger (if not already created)
resource apimLogger 'Microsoft.ApiManagement/service/loggers@2023-05-01-preview' = {
  parent: apiManagement
  name: 'appinsights-logger'
  properties: {
    loggerType: 'applicationInsights'
    credentials: {
      instrumentationKey: applicationInsightsInstrumentationKey
    }
    isBuffered: true
    resourceId: applicationInsightsId
  }
}

// Diagnostic settings for all APIs
resource apiDiagnostics 'Microsoft.ApiManagement/service/diagnostics@2023-05-01-preview' = {
  parent: apiManagement
  name: 'applicationinsights'
  properties: {
    alwaysLog: 'allErrors'
    loggerId: apimLogger.id
    sampling: {
      samplingType: 'fixed'
      percentage: 100
    }
    frontend: {
      request: {
        headers: ['Authorization', 'X-Correlation-ID']
        body: {
          bytes: 1024
        }
      }
      response: {
        headers: ['Content-Type', 'X-Response-Time']
        body: {
          bytes: 1024
        }
      }
    }
    backend: {
      request: {
        headers: ['Authorization', 'X-Correlation-ID']
        body: {
          bytes: 1024
        }
      }
      response: {
        headers: ['Content-Type', 'X-Response-Time']
        body: {
          bytes: 1024
        }
      }
    }
  }
}

// Reference existing APIs for diagnostics
resource hrApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' existing = {
  parent: apiManagement
  name: 'hr-api'
}

// API-specific diagnostic settings for HR Service
resource hrApiDiagnostics 'Microsoft.ApiManagement/service/apis/diagnostics@2023-05-01-preview' = {
  parent: hrApi
  name: 'applicationinsights'
  properties: {
    alwaysLog: 'allErrors'
    loggerId: apimLogger.id
    sampling: {
      samplingType: 'fixed'
      percentage: 100
    }
    frontend: {
      request: {
        headers: ['Authorization']
        body: {
          bytes: 512
        }
      }
      response: {
        headers: ['Content-Type']
        body: {
          bytes: 512
        }
      }
    }
    backend: {
      request: {
        headers: []
        body: {
          bytes: 512
        }
      }
      response: {
        headers: ['Content-Type']
        body: {
          bytes: 512
        }
      }
    }
  }
}

// Metrics and alerts
resource responseTimeAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${apimServiceName}-high-response-time'
  location: 'global'
  properties: {
    description: 'Alert when API response time exceeds threshold'
    severity: 2
    enabled: true
    scopes: [
      apiManagement.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighResponseTime'
          metricName: 'Duration'
          operator: 'GreaterThan'
          threshold: 5000
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: []
  }
  tags: {
    component: 'monitoring'
    service: 'api-gateway'
  }
}

resource errorRateAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${apimServiceName}-high-error-rate'
  location: 'global'
  properties: {
    description: 'Alert when API error rate exceeds threshold'
    severity: 1
    enabled: true
    scopes: [
      apiManagement.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighErrorRate'
          metricName: 'UnauthorizedRequests'
          operator: 'GreaterThan'
          threshold: 10
          timeAggregation: 'Total'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: []
  }
  tags: {
    component: 'monitoring'
    service: 'api-gateway'
  }
}

resource capacityAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${apimServiceName}-high-capacity'
  location: 'global'
  properties: {
    description: 'Alert when APIM capacity exceeds threshold'
    severity: 2
    enabled: true
    scopes: [
      apiManagement.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighCapacity'
          metricName: 'Capacity'
          operator: 'GreaterThan'
          threshold: 80
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: []
  }
  tags: {
    component: 'monitoring'
    service: 'api-gateway'
  }
}

// Custom dashboard - simplified version
resource dashboard 'Microsoft.Portal/dashboards@2020-09-01-preview' = {
  name: '${apimServiceName}-dashboard'
  location: resourceGroup().location
  properties: {
    lenses: [
      {
        order: 0
        parts: [
          {
            position: {
              x: 0
              y: 0
              rowSpan: 4
              colSpan: 12
            }
            metadata: {
              inputs: []
              type: 'Extension/HubsExtension/PartType/MarkdownPart'
              settings: {
                content: {
                  settings: {
                    content: '# API Gateway Monitoring Dashboard\n\nThis dashboard provides monitoring for the Ocelon ERP API Gateway.\n\n## Key Metrics\n- API Request Volume\n- Response Times\n- Error Rates\n- Capacity Utilization\n\nFor detailed metrics, please use Azure Monitor or Application Insights directly.'
                    title: 'API Gateway Overview'
                  }
                }
              }
            }
          }
        ]
      }
    ]
    metadata: {
      model: {
        timeRange: {
          value: {
            relative: {
              duration: 24
              timeUnit: 1
            }
          }
          type: 'MsPortalFx.Composition.Configuration.ValueTypes.TimeRange'
        }
        filterLocale: {
          value: 'en-us'
        }
        filters: {
          value: {
            MsPortalFx_TimeRange: {
              model: {
                format: 'utc'
                granularity: 'auto'
                relative: '24h'
              }
              displayCache: {
                name: 'UTC Time'
                value: 'Past 24 hours'
              }
              filteredPartIds: []
            }
          }
        }
      }
    }
  }
  tags: {
    component: 'monitoring'
    service: 'api-gateway'
  }
}

// Outputs
output dashboardId string = dashboard.id
output responseTimeAlertId string = responseTimeAlert.id
output errorRateAlertId string = errorRateAlert.id
output capacityAlertId string = capacityAlert.id
