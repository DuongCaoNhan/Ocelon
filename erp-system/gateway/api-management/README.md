# Azure API Management Setup for ERP System

This document provides step-by-step instructions for setting up Azure API Management for the Ocelon ERP Management System.

## Prerequisites

- Azure CLI installed and configured
- Azure subscription with appropriate permissions
- Resource group for ERP system resources
- Azure Active Directory tenant configured

## 1. Deploy Azure API Management Instance

### Using Azure CLI

```bash
# Set variables
RESOURCE_GROUP="ocelon-erp-rg"
APIM_NAME="ocelon-erp-apim"
LOCATION="East US"
PUBLISHER_EMAIL="admin@ocelon-erp.com"
PUBLISHER_NAME="Ocelon ERP Team"

# Create APIM instance (takes 30-45 minutes)
az apim create \
  --resource-group $RESOURCE_GROUP \
  --name $APIM_NAME \
  --location $LOCATION \
  --publisher-email $PUBLISHER_EMAIL \
  --publisher-name "$PUBLISHER_NAME" \
  --sku-name Developer
```

### Using Azure Bicep

```bicep
# Deploy using existing Bicep template
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file ../../../infrastructure/bicep/main.bicep \
  --parameters environmentName=prod
```

## 2. Import API Definitions

### Import HR Service API

```bash
# Import HR Service OpenAPI spec
az apim api import \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --api-id hr-service-api \
  --path "/v1/hr" \
  --display-name "HR Service API" \
  --protocols https \
  --specification-format OpenApi \
  --specification-path "./openapi-specs/hr-api.yaml"
```

### Import All Services

```bash
# Import all service APIs
services=("hr" "inventory" "accounting" "workflow" "notification")
ports=("5001" "5002" "5003" "8080" "3000")

for i in "${!services[@]}"; do
  service="${services[$i]}"
  port="${ports[$i]}"
  
  az apim api import \
    --resource-group $RESOURCE_GROUP \
    --service-name $APIM_NAME \
    --api-id "${service}-service-api" \
    --path "/v1/${service}" \
    --display-name "${service^} Service API" \
    --protocols https \
    --specification-format OpenApi \
    --specification-path "./openapi-specs/${service}-api.yaml" \
    --service-url "http://${service}-service.default.svc.cluster.local:${port}"
done
```

## 3. Configure Authentication

### Set up Azure AD Authentication

```bash
# Create Azure AD App Registration for API access
az ad app create \
  --display-name "Ocelon ERP API Access" \
  --identifier-uris "api://ocelon-erp-api" \
  --required-resource-accesses '[
    {
      "resourceAppId": "00000003-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
          "type": "Scope"
        }
      ]
    }
  ]'

# Get the Application ID
APP_ID=$(az ad app list --display-name "Ocelon ERP API Access" --query "[0].appId" -o tsv)

# Configure APIM with Azure AD
az apim api update \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --api-id hr-service-api \
  --authorization-server-id azure-ad-server
```

## 4. Apply Policies

### Apply Global Policy

```bash
# Apply global policy to all APIs
az apim policy create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --policy-id global \
  --policy-content @policies/global-policy.xml
```

### Apply Service-Specific Policies

```bash
# Apply HR service specific policy
az apim api policy create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --api-id hr-service-api \
  --policy-content @policies/hr-service-policy.xml

# Apply Notification service specific policy
az apim api policy create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --api-id notification-service-api \
  --policy-content @policies/notification-throttle-policy.xml
```

## 5. Configure Products and Subscriptions

### Create Products

```bash
# Create Enterprise product
az apim product create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --product-id enterprise \
  --display-name "Enterprise ERP Access" \
  --description "Full access to all ERP services" \
  --state published \
  --subscription-required true \
  --approval-required true

# Create Basic product
az apim product create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --product-id basic \
  --display-name "Basic ERP Access" \
  --description "Limited access to ERP services" \
  --state published \
  --subscription-required true \
  --approval-required false

# Add APIs to products
for api in hr-service-api inventory-service-api accounting-service-api workflow-service-api notification-service-api; do
  az apim product api add \
    --resource-group $RESOURCE_GROUP \
    --service-name $APIM_NAME \
    --product-id enterprise \
    --api-id $api
done
```

## 6. Set up Monitoring and Analytics

### Configure Application Insights

```bash
# Get Application Insights instrumentation key
AI_KEY=$(az monitor app-insights component show \
  --resource-group $RESOURCE_GROUP \
  --app ocelon-erp-appinsights \
  --query instrumentationKey -o tsv)

# Configure APIM logger
az apim logger create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --logger-id application-insights \
  --logger-type applicationInsights \
  --credentials "instrumentationKey=$AI_KEY"
```

### Set up Event Hub for Logging

```bash
# Create Event Hub namespace and hub
az eventhubs namespace create \
  --resource-group $RESOURCE_GROUP \
  --name ocelon-erp-eventhub \
  --location $LOCATION

az eventhubs eventhub create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name ocelon-erp-eventhub \
  --name api-logs

# Configure APIM Event Hub logger
az apim logger create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --logger-id erp-event-hub \
  --logger-type azureEventHub \
  --credentials "name=ocelon-erp-eventhub;connectionString=<connection-string>"
```

## 7. Configure Custom Domains and SSL

### Set up Custom Domain

```bash
# Upload SSL certificate
az apim certificate create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --certificate-id ssl-cert \
  --certificate-path "/path/to/certificate.pfx" \
  --certificate-password "certificate-password"

# Configure custom domain
az apim hostname-configuration create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --hostname-type Proxy \
  --hostname "api.ocelon-erp.com" \
  --certificate-id ssl-cert
```

## 8. Environment-Specific Configuration

### Development Environment

```bash
# Set backend URLs for development
az apim backend create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --backend-id hr-service-dev \
  --url "http://hr-service-dev.default.svc.cluster.local:5001" \
  --protocol http
```

### Production Environment

```bash
# Set backend URLs for production
az apim backend create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --backend-id hr-service-prod \
  --url "http://hr-service.default.svc.cluster.local:5001" \
  --protocol http
```

## 9. Testing and Validation

### Test API Access

```bash
# Get subscription key
SUBSCRIPTION_KEY=$(az apim subscription list \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --query "[0].primaryKey" -o tsv)

# Test HR API
curl -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY" \
     -H "Authorization: Bearer <jwt-token>" \
     "https://$APIM_NAME.azure-api.net/v1/hr/employees"
```

### Health Check

```bash
# Check APIM status
az apim show \
  --resource-group $RESOURCE_GROUP \
  --name $APIM_NAME \
  --query "provisioningState"
```

## 10. CI/CD Integration

### Azure DevOps Pipeline

```yaml
# azure-pipelines-apim.yml
trigger:
  branches:
    include:
    - main
  paths:
    include:
    - gateway/api-management/*

stages:
- stage: DeployAPIM
  jobs:
  - job: ImportAPIs
    steps:
    - task: AzureCLI@2
      inputs:
        azureSubscription: 'ERP-Service-Connection'
        scriptType: 'bash'
        scriptLocation: 'inlineScript'
        inlineScript: |
          # Import updated API definitions
          az apim api import \
            --resource-group $(ResourceGroup) \
            --service-name $(APIMName) \
            --api-id hr-service-api \
            --specification-path "gateway/api-management/openapi-specs/hr-api.yaml"
```

### GitHub Actions

```yaml
name: Deploy APIM Configuration
on:
  push:
    branches: [main]
    paths: ['gateway/api-management/**']

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Deploy API definitions
      run: |
        cd gateway/api-management
        ./deploy-apis.sh
```

## 11. Security Best Practices

### Configure IP Restrictions

```bash
# Allow only specific IP ranges
az apim policy create \
  --resource-group $RESOURCE_GROUP \
  --service-name $APIM_NAME \
  --policy-id ip-restriction \
  --policy-content '<policies>
    <inbound>
      <ip-filter action="allow">
        <address-range from="10.0.0.0" to="10.255.255.255" />
        <address-range from="192.168.0.0" to="192.168.255.255" />
      </ip-filter>
    </inbound>
  </policies>'
```

### Set up WAF (Web Application Firewall)

```bash
# Deploy Application Gateway with WAF
az network application-gateway create \
  --resource-group $RESOURCE_GROUP \
  --name ocelon-erp-appgw \
  --location $LOCATION \
  --capacity 2 \
  --sku WAF_v2 \
  --frontend-port 80 \
  --http-settings-cookie-based-affinity Disabled \
  --vnet-name ocelon-erp-vnet \
  --subnet appgw-subnet \
  --public-ip-address ocelon-erp-pip
```

## 12. Monitoring and Alerting

### Set up Alerts

```bash
# Create alert for high error rate
az monitor metrics alert create \
  --resource-group $RESOURCE_GROUP \
  --name "APIM High Error Rate" \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.ApiManagement/service/$APIM_NAME" \
  --condition "avg Percentage4XX > 10" \
  --description "Alert when error rate exceeds 10%"
```

## 13. Backup and Disaster Recovery

### Backup APIM Configuration

```bash
# Export APIM configuration
az apim backup \
  --resource-group $RESOURCE_GROUP \
  --name $APIM_NAME \
  --storage-account-name ocelonerpstorage \
  --storage-account-container apim-backups \
  --backup-name "apim-backup-$(date +%Y%m%d)"
```

This setup provides a comprehensive API Management solution for the ERP system with proper authentication, monitoring, and security configurations.
