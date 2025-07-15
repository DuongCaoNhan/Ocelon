@description('APIM service name')
param apimServiceName string

// Reference existing APIM service
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' existing = {
  name: apimServiceName
}

// Products
resource erpProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apiManagement
  name: 'erp-system'
  properties: {
    displayName: 'ERP System APIs'
    description: 'Complete ERP system API access including HR, Inventory, Accounting, Workflow, and Notifications'
    state: 'published'
    subscriptionRequired: true
    approvalRequired: true
    subscriptionsLimit: 100
    terms: 'By subscribing to this product, you agree to the terms of service and acceptable use policy.'
  }
}

resource hrProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apiManagement
  name: 'hr-service'
  properties: {
    displayName: 'HR Service APIs'
    description: 'Human Resources management APIs for employee data and operations'
    state: 'published'
    subscriptionRequired: true
    approvalRequired: false
    subscriptionsLimit: 50
  }
}

resource inventoryProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apiManagement
  name: 'inventory-service'
  properties: {
    displayName: 'Inventory Service APIs'
    description: 'Inventory management APIs for product and stock operations'
    state: 'published'
    subscriptionRequired: true
    approvalRequired: false
    subscriptionsLimit: 50
  }
}

resource accountingProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apiManagement
  name: 'accounting-service'
  properties: {
    displayName: 'Accounting Service APIs'
    description: 'Financial and accounting management APIs'
    state: 'published'
    subscriptionRequired: true
    approvalRequired: true
    subscriptionsLimit: 25
  }
}

resource workflowProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apiManagement
  name: 'workflow-service'
  properties: {
    displayName: 'Workflow Service APIs'
    description: 'Business process and workflow management APIs'
    state: 'published'
    subscriptionRequired: true
    approvalRequired: false
    subscriptionsLimit: 30
  }
}

resource notificationProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apiManagement
  name: 'notification-service'
  properties: {
    displayName: 'Notification Service APIs'
    description: 'Real-time notification and messaging APIs'
    state: 'published'
    subscriptionRequired: true
    approvalRequired: false
    subscriptionsLimit: 100
  }
}

// API Imports
resource hrApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apiManagement
  name: 'hr-api'
  properties: {
    displayName: 'HR Service API'
    description: 'Human Resources management API'
    serviceUrl: '{{hr-service-backend-url}}'
    path: 'hr'
    protocols: ['https']
    subscriptionRequired: true
    format: 'openapi+json-link'
    value: 'https://raw.githubusercontent.com/ocelon/erp-gateway/main/openapi-specs/hr-api.yaml'
  }
}

resource inventoryApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apiManagement
  name: 'inventory-api'
  properties: {
    displayName: 'Inventory Service API'
    description: 'Inventory management API'
    serviceUrl: '{{inventory-service-backend-url}}'
    path: 'inventory'
    protocols: ['https']
    subscriptionRequired: true
    format: 'openapi+json-link'
    value: 'https://raw.githubusercontent.com/ocelon/erp-gateway/main/openapi-specs/inventory-api.yaml'
  }
}

resource accountingApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apiManagement
  name: 'accounting-api'
  properties: {
    displayName: 'Accounting Service API'
    description: 'Financial and accounting management API'
    serviceUrl: '{{accounting-service-backend-url}}'
    path: 'accounting'
    protocols: ['https']
    subscriptionRequired: true
    format: 'openapi+json-link'
    value: 'https://raw.githubusercontent.com/ocelon/erp-gateway/main/openapi-specs/accounting-api.yaml'
  }
}

resource workflowApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apiManagement
  name: 'workflow-api'
  properties: {
    displayName: 'Workflow Service API'
    description: 'Business process and workflow management API'
    serviceUrl: '{{workflow-service-backend-url}}'
    path: 'workflow'
    protocols: ['https']
    subscriptionRequired: true
    format: 'openapi+json-link'
    value: 'https://raw.githubusercontent.com/ocelon/erp-gateway/main/openapi-specs/workflow-api.yaml'
  }
}

resource notificationApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apiManagement
  name: 'notification-api'
  properties: {
    displayName: 'Notification Service API'
    description: 'Real-time notification and messaging API'
    serviceUrl: '{{notification-service-backend-url}}'
    path: 'notifications'
    protocols: ['https']
    subscriptionRequired: true
    format: 'openapi+json-link'
    value: 'https://raw.githubusercontent.com/ocelon/erp-gateway/main/openapi-specs/notification-api.yaml'
  }
}

// Product-API associations
resource erpProductHrApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: erpProduct
  name: 'hr-api'
  dependsOn: [hrApi]
}

resource erpProductInventoryApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: erpProduct
  name: 'inventory-api'
  dependsOn: [inventoryApi]
}

resource erpProductAccountingApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: erpProduct
  name: 'accounting-api'
  dependsOn: [accountingApi]
}

resource erpProductWorkflowApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: erpProduct
  name: 'workflow-api'
  dependsOn: [workflowApi]
}

resource erpProductNotificationApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: erpProduct
  name: 'notification-api'
  dependsOn: [notificationApi]
}

// Individual product-API associations
resource hrProductApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: hrProduct
  name: 'hr-api'
  dependsOn: [hrApi]
}

resource inventoryProductApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: inventoryProduct
  name: 'inventory-api'
  dependsOn: [inventoryApi]
}

resource accountingProductApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: accountingProduct
  name: 'accounting-api'
  dependsOn: [accountingApi]
}

resource workflowProductApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: workflowProduct
  name: 'workflow-api'
  dependsOn: [workflowApi]
}

resource notificationProductApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: notificationProduct
  name: 'notification-api'
  dependsOn: [notificationApi]
}

// User groups
resource developersGroup 'Microsoft.ApiManagement/service/groups@2023-05-01-preview' = {
  parent: apiManagement
  name: 'developers'
  properties: {
    displayName: 'Developers'
    description: 'Developer group with access to development APIs'
    type: 'custom'
  }
}

resource partnersGroup 'Microsoft.ApiManagement/service/groups@2023-05-01-preview' = {
  parent: apiManagement
  name: 'partners'
  properties: {
    displayName: 'Partners'
    description: 'Partner organizations with limited API access'
    type: 'custom'
  }
}

resource adminGroup 'Microsoft.ApiManagement/service/groups@2023-05-01-preview' = {
  parent: apiManagement
  name: 'administrators'
  properties: {
    displayName: 'Administrators'
    description: 'System administrators with full API access'
    type: 'custom'
  }
}

// Group product associations
resource developersGroupErpProduct 'Microsoft.ApiManagement/service/products/groups@2023-05-01-preview' = {
  parent: erpProduct
  name: 'developers'
  dependsOn: [developersGroup]
}

resource adminGroupErpProduct 'Microsoft.ApiManagement/service/products/groups@2023-05-01-preview' = {
  parent: erpProduct
  name: 'administrators'
  dependsOn: [adminGroup]
}

// Outputs
output erpProductId string = erpProduct.id
output hrApiId string = hrApi.id
output inventoryApiId string = inventoryApi.id
output accountingApiId string = accountingApi.id
output workflowApiId string = workflowApi.id
output notificationApiId string = notificationApi.id
