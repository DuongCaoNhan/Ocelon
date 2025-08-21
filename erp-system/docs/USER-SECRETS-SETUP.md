# User Secrets Configuration Guide

This guide helps you set up user secrets for local development of the ERP system.

## Setup Instructions

### 1. Initialize User Secrets for Each Service

Run these commands from the root of the repository:

```bash
# AIAgent Service
dotnet user-secrets init --project erp-system/src/AIAgentService/AIAgentService.API
dotnet user-secrets set --project erp-system/src/AIAgentService/AIAgentService.API "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=AIAgentDB_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"

# HR Service  
dotnet user-secrets init --project erp-system/src/HRService/HRService.API
dotnet user-secrets set --project erp-system/src/HRService/HRService.API "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=HRServiceDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"

# Audit Service
dotnet user-secrets init --project erp-system/src/AuditService/AuditService.API
dotnet user-secrets set --project erp-system/src/AuditService/AuditService.API "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=AuditServiceDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"

# Gateway
dotnet user-secrets init --project erp-system/gateway/self-hosted-gateway/ocelot
```

### 2. Set Common Secrets

For each service, set these common development secrets:

```bash
# Replace [PROJECT_PATH] with the actual project path

# JWT Configuration
dotnet user-secrets set --project [PROJECT_PATH] "Jwt:Key" "your-super-secret-development-key-that-is-at-least-256-bits-long-for-jwt-signing"
dotnet user-secrets set --project [PROJECT_PATH] "Jwt:Issuer" "https://localhost:5000"
dotnet user-secrets set --project [PROJECT_PATH] "Jwt:Audience" "erp-system-dev"

# Redis
dotnet user-secrets set --project [PROJECT_PATH] "ConnectionStrings:Redis" "localhost:6379"

# Azure OpenAI (for AIAgent Service)
dotnet user-secrets set --project erp-system/src/AIAgentService/AIAgentService.API "AzureOpenAI:Endpoint" "https://your-dev-openai-resource.openai.azure.com/"
dotnet user-secrets set --project erp-system/src/AIAgentService/AIAgentService.API "AzureOpenAI:ApiKey" "your-azure-openai-api-key-here"
```

### 3. Optional Azure Services for Local Testing

If you want to test with Azure services locally:

```bash
# Application Insights
dotnet user-secrets set --project [PROJECT_PATH] "ApplicationInsights:ConnectionString" "InstrumentationKey=your-dev-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"

# Service Bus
dotnet user-secrets set --project [PROJECT_PATH] "Azure:ServiceBus:ConnectionString" "Endpoint=sb://your-dev-servicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"

# Key Vault (for testing Key Vault integration)
dotnet user-secrets set --project [PROJECT_PATH] "Azure:KeyVault:VaultUrl" "https://your-dev-keyvault.vault.azure.net/"
```

## Managing User Secrets

### List Current Secrets
```bash
dotnet user-secrets list --project [PROJECT_PATH]
```

### Remove a Secret
```bash
dotnet user-secrets remove --project [PROJECT_PATH] "SecretKey"
```

### Clear All Secrets
```bash
dotnet user-secrets clear --project [PROJECT_PATH]
```

## Security Notes

- User secrets are stored locally on your development machine
- They are NOT committed to source control
- Use them only for development - never for production
- Each developer needs to set up their own user secrets
- For production, use Azure Key Vault with managed identities

## Troubleshooting

### Invalid Secret Key Error
Make sure your JWT key is at least 256 bits (32 characters) long.

### Database Connection Issues
Ensure SQL Server LocalDB is installed and running:
```bash
SqlLocalDB start mssqllocaldb
```

### Azure Service Authentication
For Azure services, you may need to authenticate locally:
```bash
az login
```