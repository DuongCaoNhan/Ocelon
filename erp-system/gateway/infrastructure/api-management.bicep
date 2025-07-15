@description('Environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

@description('API Management service name')
param apimServiceName string = 'ocelon-erp-apim-${environmentName}'

@description('Publisher name for APIM')
param publisherName string = 'Ocelon ERP'

@description('Publisher email for APIM')
param publisherEmail string = 'admin@ocelon.com'

@description('SKU name for APIM')
@allowed(['Developer', 'Standard', 'Premium'])
param skuName string = environmentName == 'prod' ? 'Premium' : 'Developer'

@description('SKU capacity for APIM')
param skuCapacity int = environmentName == 'prod' ? 2 : 1

@description('Virtual network subnet ID for APIM (Premium SKU only)')
param subnetId string = ''

var apimConfiguration = {
  virtualNetworkType: skuName == 'Premium' && !empty(subnetId) ? 'Internal' : 'None'
  virtualNetworkConfiguration: skuName == 'Premium' && !empty(subnetId) ? {
    subnetResourceId: subnetId
  } : null
}

// API Management Service
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apimServiceName
  location: location
  sku: {
    name: skuName
    capacity: skuCapacity
  }
  properties: {
    publisherName: publisherName
    publisherEmail: publisherEmail
    virtualNetworkType: apimConfiguration.virtualNetworkType
    virtualNetworkConfiguration: apimConfiguration.?virtualNetworkConfiguration
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Ssl30': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'false'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    environment: environmentName
    project: 'ocelon-erp'
    component: 'api-gateway'
  }
}

// Application Insights for APIM
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${apimServiceName}-appinsights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
  tags: {
    environment: environmentName
    project: 'ocelon-erp'
    component: 'monitoring'
  }
}

// Application Insights Logger
resource apimLogger 'Microsoft.ApiManagement/service/loggers@2023-05-01-preview' = {
  parent: apiManagement
  name: 'appinsights-logger'
  properties: {
    loggerType: 'applicationInsights'
    credentials: {
      instrumentationKey: applicationInsights.properties.InstrumentationKey
    }
    isBuffered: true
    resourceId: applicationInsights.id
  }
}

// Diagnostic settings for all APIs
resource diagnostics 'Microsoft.ApiManagement/service/diagnostics@2023-05-01-preview' = {
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
        headers: ['Authorization']
        body: {
          bytes: 1024
        }
      }
      response: {
        headers: ['Content-Type']
        body: {
          bytes: 1024
        }
      }
    }
    backend: {
      request: {
        headers: ['Authorization']
        body: {
          bytes: 1024
        }
      }
      response: {
        headers: ['Content-Type']
        body: {
          bytes: 1024
        }
      }
    }
  }
}

// Key Vault for storing secrets
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${apimServiceName}-kv'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    accessPolicies: [
      {
        tenantId: tenant().tenantId
        objectId: apiManagement.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
    enabledForTemplateDeployment: true
    enableRbacAuthorization: false
  }
  tags: {
    environment: environmentName
    project: 'ocelon-erp'
    component: 'security'
  }
}

// Named values for backend URLs
resource hrServiceBackend 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apiManagement
  name: 'hr-service-backend-url'
  properties: {
    displayName: 'HR-Service-Backend-URL'
    value: environmentName == 'prod' 
      ? 'https://hr-service-prod.ocelon.com'
      : 'https://hr-service-${environmentName}.ocelon.com'
    secret: false
  }
}

resource inventoryServiceBackend 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apiManagement
  name: 'inventory-service-backend-url'
  properties: {
    displayName: 'Inventory-Service-Backend-URL'
    value: environmentName == 'prod' 
      ? 'https://inventory-service-prod.ocelon.com'
      : 'https://inventory-service-${environmentName}.ocelon.com'
    secret: false
  }
}

resource accountingServiceBackend 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apiManagement
  name: 'accounting-service-backend-url'
  properties: {
    displayName: 'Accounting-Service-Backend-URL'
    value: environmentName == 'prod' 
      ? 'https://accounting-service-prod.ocelon.com'
      : 'https://accounting-service-${environmentName}.ocelon.com'
    secret: false
  }
}

resource workflowServiceBackend 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apiManagement
  name: 'workflow-service-backend-url'
  properties: {
    displayName: 'Workflow-Service-Backend-URL'
    value: environmentName == 'prod' 
      ? 'https://workflow-service-prod.ocelon.com'
      : 'https://workflow-service-${environmentName}.ocelon.com'
    secret: false
  }
}

resource notificationServiceBackend 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apiManagement
  name: 'notification-service-backend-url'
  properties: {
    displayName: 'Notification-Service-Backend-URL'
    value: environmentName == 'prod' 
      ? 'https://notification-service-prod.ocelon.com'
      : 'https://notification-service-${environmentName}.ocelon.com'
    secret: false
  }
}

// JWT signing key from Key Vault
resource jwtSigningKey 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apiManagement
  name: 'jwt-signing-key'
  properties: {
    displayName: 'JWT-Signing-Key'
    keyVault: {
      secretIdentifier: '${keyVault.properties.vaultUri}secrets/jwt-signing-key'
    }
    secret: true
  }
}

// Outputs
output apimServiceName string = apiManagement.name
output apimServiceId string = apiManagement.id
output apimGatewayUrl string = apiManagement.properties.gatewayUrl
output apimManagementUrl string = apiManagement.properties.managementApiUrl
output apimPortalUrl string = apiManagement.properties.portalUrl
output apimDeveloperPortalUrl string = apiManagement.properties.developerPortalUrl
output applicationInsightsId string = applicationInsights.id
output applicationInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
output keyVaultName string = keyVault.name
output keyVaultId string = keyVault.id
