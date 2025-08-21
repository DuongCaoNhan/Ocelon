# Azure Key Vault Configuration Guide

This guide explains how to configure Azure Key Vault for the ERP system microservices.

## Overview

The ERP system uses Azure Key Vault to store sensitive configuration values in non-development environments. This includes:

- Database connection strings
- JWT signing keys
- Azure OpenAI API keys
- Service Bus connection strings
- Application Insights instrumentation keys

## Secret Naming Convention

Key Vault secrets use the following naming convention:
- Replace `:` with `--` in configuration keys
- Use kebab-case for multi-word secrets

Examples:
- `ConnectionStrings:DefaultConnection` → `ConnectionStrings--DefaultConnection`
- `Jwt:Key` → `Jwt--Key`
- `AzureOpenAI:ApiKey` → `AzureOpenAI--ApiKey`

## Required Secrets in Key Vault

### Common Secrets (All Services)

```bash
# Database connections
ConnectionStrings--DefaultConnection
ConnectionStrings--Redis

# JWT Configuration
Jwt--Key
Jwt--Issuer
Jwt--Audience

# Application Insights
ApplicationInsights--ConnectionString

# Service Bus
Azure--ServiceBus--ConnectionString
```

### AIAgent Service Specific

```bash
# Azure OpenAI
AzureOpenAI--Endpoint
AzureOpenAI--ApiKey
AzureOpenAI--DeploymentName
```

## Setting Up Key Vault

### 1. Create Key Vault

```bash
# Create resource group
az group create --name rg-ocelon-erp --location eastus

# Create Key Vault
az keyvault create --name kv-ocelon-erp-prod --resource-group rg-ocelon-erp --location eastus
```

### 2. Set Secrets

```bash
# Set database connection
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "ConnectionStrings--DefaultConnection" --value "Server=tcp:ocelon-sql-prod.database.windows.net,1433;Initial Catalog=ERPDatabase;Authentication=Active Directory Managed Identity;Encrypt=True;"

# Set JWT key
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "Jwt--Key" --value "your-super-secure-production-jwt-key-at-least-256-bits-long"

# Set Redis connection
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "ConnectionStrings--Redis" --value "ocelon-redis-prod.redis.cache.windows.net:6380,password=your-redis-key,ssl=True,abortConnect=False"

# Set Application Insights
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "ApplicationInsights--ConnectionString" --value "InstrumentationKey=your-app-insights-key;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/"

# Set Service Bus
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "Azure--ServiceBus--ConnectionString" --value "Endpoint=sb://ocelon-servicebus-prod.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-servicebus-key"

# AIAgent Service - Azure OpenAI
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "AzureOpenAI--Endpoint" --value "https://ocelon-openai-prod.openai.azure.com/"
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "AzureOpenAI--ApiKey" --value "your-azure-openai-api-key"
az keyvault secret set --vault-name kv-ocelon-erp-prod --name "AzureOpenAI--DeploymentName" --value "gpt-4-turbo"
```

### 3. Configure Access Policies

Grant the services' managed identities access to Key Vault:

```bash
# For Container Apps (using managed identity)
az keyvault set-policy --name kv-ocelon-erp-prod --object-id <managed-identity-principal-id> --secret-permissions get list
```

## Managed Identity Setup

Each service should use a managed identity to authenticate with Key Vault:

### System-Assigned Managed Identity
```bash
# Enable for Container App
az containerapp identity assign --name <app-name> --resource-group <resource-group>
```

### User-Assigned Managed Identity
```bash
# Create user-assigned identity
az identity create --name id-ocelon-erp --resource-group rg-ocelon-erp

# Assign to Container App
az containerapp identity assign --name <app-name> --resource-group <resource-group> --user-assigned <identity-resource-id>
```

## Configuration in Services

The services automatically load Key Vault secrets when:
1. Not running in Development environment
2. `Azure:KeyVault:VaultUrl` is configured in appsettings

Example appsettings.Production.json:
```json
{
  "Azure": {
    "KeyVault": {
      "VaultUrl": "https://kv-ocelon-erp-prod.vault.azure.net/"
    }
  }
}
```

## Local Development Override

For local development, you can test Key Vault integration by:

1. Setting up Azure CLI authentication:
   ```bash
   az login
   ```

2. Adding Key Vault URL to user secrets:
   ```bash
   dotnet user-secrets set "Azure:KeyVault:VaultUrl" "https://kv-ocelon-erp-dev.vault.azure.net/"
   ```

## Troubleshooting

### Common Issues

1. **Authentication Failed**: Ensure managed identity has proper Key Vault access policies
2. **Secret Not Found**: Check secret naming convention (`:` → `--`)
3. **Network Access**: Ensure Key Vault allows access from Container Apps subnet

### Monitoring

Monitor Key Vault access through:
- Azure Monitor Key Vault insights
- Application Insights telemetry
- Service logs for Key Vault operation failures

## Security Best Practices

1. Use separate Key Vaults for different environments
2. Enable Key Vault logging and monitoring
3. Use managed identities instead of connection strings
4. Regularly rotate secrets
5. Set appropriate access policies (principle of least privilege)
6. Enable soft delete and purge protection