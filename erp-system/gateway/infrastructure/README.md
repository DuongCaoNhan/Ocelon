# API Gateway Infrastructure

This directory contains Bicep templates for deploying the Azure API Management infrastructure for the ERP system.

## Templates

- `api-management.bicep` - Main APIM instance
- `api-management-policies.bicep` - Policy configurations
- `api-management-products.bicep` - Product and subscription management
- `monitoring.bicep` - Monitoring and analytics setup

## Deployment

```bash
# Deploy APIM infrastructure
az deployment group create \
  --resource-group ocelon-erp-rg \
  --template-file api-management.bicep \
  --parameters environmentName=prod
```
