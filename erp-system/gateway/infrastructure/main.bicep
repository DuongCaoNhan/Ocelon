@description('Environment name')
param environmentName string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

// Deploy API Management
module apim 'api-management.bicep' = {
  name: 'apim-deployment'
  params: {
    environmentName: environmentName
    location: location
  }
}

// Deploy API products and APIs
module products 'api-management-products.bicep' = {
  name: 'products-deployment'
  params: {
    apimServiceName: apim.outputs.apimServiceName
  }
}

// Deploy monitoring
module monitoring 'monitoring.bicep' = {
  name: 'monitoring-deployment'
  params: {
    apimServiceName: apim.outputs.apimServiceName
    applicationInsightsId: apim.outputs.applicationInsightsId
    applicationInsightsInstrumentationKey: apim.outputs.applicationInsightsInstrumentationKey
  }
  dependsOn: [
    products
  ]
}

// Outputs
output apimServiceName string = apim.outputs.apimServiceName
output apimGatewayUrl string = apim.outputs.apimGatewayUrl
output apimDeveloperPortalUrl string = apim.outputs.apimDeveloperPortalUrl
output keyVaultName string = apim.outputs.keyVaultName
output dashboardId string = monitoring.outputs.dashboardId
