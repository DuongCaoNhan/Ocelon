// Azure Infrastructure for ERP Audit Service
// Deploys all necessary Azure resources for centralized audit logging

@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('Unique suffix for resource naming')
param resourceSuffix string = uniqueString(resourceGroup().id)

@description('SQL Server administrator username')
param sqlAdminUsername string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('Service Bus namespace name')
param serviceBusNamespace string = 'sb-erp-audit-${environment}-${resourceSuffix}'

@description('Application Insights name')
param appInsightsName string = 'ai-erp-audit-${environment}-${resourceSuffix}'

@description('Log Analytics workspace name')
param logAnalyticsName string = 'law-erp-audit-${environment}-${resourceSuffix}'

@description('Key Vault name')
param keyVaultName string = 'kv-erp-audit-${environment}-${resourceSuffix}'

@description('Container Registry name')
param acrName string = 'acrerp${environment}${resourceSuffix}'

@description('Container App Environment name')
param containerAppEnvName string = 'cae-erp-audit-${environment}-${resourceSuffix}'

@description('Tags to apply to all resources')
param tags object = {
  Environment: environment
  Application: 'ERP-AuditService'
  ManagedBy: 'Bicep'
}

// Variables
var sqlServerName = 'sql-erp-audit-${environment}-${resourceSuffix}'
var sqlDatabaseName = 'sqldb-erp-audit-${environment}'
var auditServiceName = 'ca-erp-audit-${environment}-${resourceSuffix}'

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: environment == 'prod' ? 90 : 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
      disableLocalAuth: false
    }
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Key Vault for storing secrets
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: true
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    accessPolicies: []
  }
}

// Service Bus Namespace for audit events
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespace
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    zoneRedundant: environment == 'prod'
  }
}

// Service Bus Queue for audit events
resource auditQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'audit-events'
  properties: {
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P14D' // 14 days
    enablePartitioning: false
    maxDeliveryCount: 5
    autoDeleteOnIdle: 'P30D' // 30 days
  }
}

// Service Bus Dead Letter Queue (automatically created with main queue)

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  tags: tags
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database for audit logs
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  tags: tags
  sku: {
    name: environment == 'prod' ? 'S2' : 'S0'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: environment == 'prod' ? 268435456000 : 2147483648 // 250GB for prod, 2GB for dev
    zoneRedundant: environment == 'prod'
    readScale: environment == 'prod' ? 'Enabled' : 'Disabled'
  }
}

// SQL Server firewall rule for Azure services
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  tags: tags
  sku: {
    name: environment == 'prod' ? 'Premium' : 'Basic'
  }
  properties: {
    adminUserEnabled: false
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 30
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: environment == 'prod' ? 'Enabled' : 'Disabled'
  }
}

// Managed Identity for the audit service
resource auditServiceIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-erp-audit-${environment}-${resourceSuffix}'
  location: location
  tags: tags
}

// Container Apps Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        workloadProfileType: 'Consumption'
        name: 'Consumption'
      }
    ]
  }
}

// Container App for Audit Service
resource auditServiceApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: auditServiceName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${auditServiceIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      secrets: [
        {
          name: 'sql-connection-string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'application-insights-connection-string'
          value: appInsights.properties.ConnectionString
        }
      ]
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: auditServiceIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'audit-service'
          image: '${containerRegistry.properties.loginServer}/erp/audit-service:latest'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environment == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'ConnectionStrings__AuditDatabase'
              secretRef: 'sql-connection-string'
            }
            {
              name: 'ConnectionStrings__ApplicationInsights'
              secretRef: 'application-insights-connection-string'
            }
            {
              name: 'AzureServiceBus__Namespace'
              value: '${serviceBus.name}.servicebus.windows.net'
            }
            {
              name: 'AzureServiceBus__AuditQueue'
              value: 'audit-events'
            }
            {
              name: 'SERVICE_NAME'
              value: 'AuditService'
            }
          ]
          resources: {
            cpu: environment == 'prod' ? 1.0 : 0.5
            memory: environment == 'prod' ? '2Gi' : '1Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health/live'
                port: 8080
              }
              initialDelaySeconds: 30
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
              }
              initialDelaySeconds: 5
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: environment == 'prod' ? 2 : 1
        maxReplicas: environment == 'prod' ? 10 : 3
        rules: [
          {
            name: 'http-scale-rule'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}

// Role assignments for the managed identity

// SQL Database Contributor role for the managed identity
resource sqlDbContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sqlServer.id, auditServiceIdentity.id, 'b24988ac-6180-42a0-ab88-20f7382dd24c')
  scope: sqlServer
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c') // SQL DB Contributor
    principalId: auditServiceIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Service Bus Data Owner role for the managed identity
resource serviceBusDataOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBus.id, auditServiceIdentity.id, '090c5cfd-751d-490a-894a-3ce6f1109419')
  scope: serviceBus
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419') // Service Bus Data Owner
    principalId: auditServiceIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Key Vault Secrets User role for the managed identity
resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, auditServiceIdentity.id, '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: auditServiceIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Container Registry AcrPull role for the managed identity
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, auditServiceIdentity.id, '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: auditServiceIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Store secrets in Key Vault
resource sqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'sql-connection-string'
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}

resource appInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'application-insights-connection-string'
  properties: {
    value: appInsights.properties.ConnectionString
  }
}

// Outputs
output auditServiceUrl string = 'https://${auditServiceApp.properties.configuration.ingress.fqdn}'
output serviceBusNamespace string = serviceBus.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output applicationInsightsConnectionString string = appInsights.properties.ConnectionString
output keyVaultName string = keyVault.name
output containerRegistryLoginServer string = containerRegistry.properties.loginServer
output managedIdentityPrincipalId string = auditServiceIdentity.properties.principalId
output managedIdentityClientId string = auditServiceIdentity.properties.clientId
