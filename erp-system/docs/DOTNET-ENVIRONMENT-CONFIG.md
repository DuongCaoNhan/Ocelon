# .NET Services Environment Configuration Guide

This document explains the environment-specific configurations for all .NET services in the ERP system.

## 🏗️ Services with Environment Configs

| Service | Port | Base Path | Health Check |
|---------|------|-----------|--------------|
| **HRService** | 5001 | `/api/v1/hr` | `/health` |
| **AccountingService** | 5003 | `/api/v1/accounting` | `/health` |
| **InventoryService** | 5002 | `/api/v1/inventory` | `/health` |
| **AuditService** | 5004 | `/api/v1/audit` | `/health` |
| **AIAgentService** | 5005 | `/api/v1/ai` | `/health` |
| **CopilotAgentService** | 5006 | `/api/v1/copilot` | `/health` |

## 📂 Configuration Files Structure

Each service has the following configuration files:

```
ServiceName.API/
├── appsettings.json                 # Base configuration
├── appsettings.Development.json     # Development overrides
├── appsettings.Staging.json         # Staging overrides
└── appsettings.Production.json      # Production overrides
```

## 🌟 Key Configuration Differences by Environment

### Development
- **Database**: Local SQL Server or LocalDB
- **Logging**: Debug level, detailed errors enabled
- **JWT**: Long expiration (60 minutes), weak keys for testing
- **Swagger**: Enabled with XML comments
- **Rate Limiting**: Disabled for easier testing
- **Caching**: In-memory, short TTL (5 minutes)
- **Azure Services**: Disabled or mock endpoints

### Staging
- **Database**: Azure SQL Database (staging instance)
- **Logging**: Information level
- **JWT**: Medium expiration (30 minutes), secure keys
- **Swagger**: Enabled for testing
- **Rate Limiting**: Moderate limits
- **Caching**: Redis, medium TTL (15-20 minutes)
- **Azure Services**: Staging Azure resources

### Production
- **Database**: Azure SQL Database (production instance)
- **Logging**: Warning level only, no sensitive data
- **JWT**: Short expiration (15 minutes), strong keys
- **Swagger**: Disabled for security
- **Rate Limiting**: Strict limits
- **Caching**: Redis, long TTL (30-60 minutes)
- **Azure Services**: Production Azure resources

## 🔧 Environment Variables Setup

### 1. Copy Environment Template
```bash
cp .env.template .env
```

### 2. Fill in Values
Edit `.env` file with your actual values for each environment.

### 3. Load Environment Variables

#### Windows (PowerShell)
```powershell
# Load variables from .env file
Get-Content .env | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
    }
}
```

#### Linux/macOS
```bash
export $(cat .env | xargs)
```

## 🚀 Running Services in Different Environments

### Set Environment Variable
```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_ENVIRONMENT=Staging  
set ASPNETCORE_ENVIRONMENT=Production

# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_ENVIRONMENT=Staging
export ASPNETCORE_ENVIRONMENT=Production
```

### Run Service
```bash
cd src/ServiceName/ServiceName.API
dotnet run
```

### Using Launch Profiles (Visual Studio)
Each service should have `Properties/launchSettings.json` with profiles for each environment.

## 🔒 Security Considerations

### JWT Settings
- **Development**: Use weak keys, long expiration for convenience
- **Staging**: Use secure keys, medium expiration for testing
- **Production**: Use strong keys, short expiration for security

### Database Connections
- **Development**: Local connections, trusted authentication
- **Staging/Production**: Encrypted connections, strong authentication

### Logging
- **Development**: Log everything including sensitive data
- **Staging**: Log important events, some sensitive data
- **Production**: Log only warnings/errors, no sensitive data

## 📊 Monitoring and Health Checks

### Health Check Endpoints
All services expose health checks at `/health`:

```bash
# Check service health
curl http://localhost:5001/health  # HR Service
curl http://localhost:5002/health  # Inventory Service
curl http://localhost:5003/health  # Accounting Service
```

### Application Insights
Each service sends telemetry to environment-specific Application Insights instances.

## 🛠️ Service-Specific Configuration

### HRService
- **Port**: 5001
- **Special Features**: Employee data handling, sensitive information
- **Rate Limits**: 50 req/min (prod), 100 req/min (staging)

### AccountingService  
- **Port**: 5003
- **Special Features**: Financial data, audit trails
- **Rate Limits**: 30 req/min (prod), 75 req/min (staging)

### InventoryService
- **Port**: 5002
- **Special Features**: Real-time inventory tracking
- **Rate Limits**: 50 req/min (prod), 100 req/min (staging)

### AuditService
- **Port**: 5004
- **Special Features**: System-wide audit logging
- **Rate Limits**: 100 req/min (prod), 200 req/min (staging)

### AIAgentService
- **Port**: 5005
- **Special Features**: Azure OpenAI integration
- **Rate Limits**: 20 req/min (prod), 50 req/min (staging)
- **AI Settings**: Token limits, temperature settings vary by environment

### CopilotAgentService
- **Port**: 5006
- **Special Features**: Copilot chat functionality
- **Rate Limits**: 15 req/min (prod), 30 req/min (staging)
- **Copilot Settings**: Context tokens, conversation history

## 🔄 Configuration Validation

### Validate Configuration Loading
```csharp
// Add to Program.cs in each service
var config = builder.Configuration;
var environment = builder.Environment.EnvironmentName;

Console.WriteLine($"Running in {environment} environment");
Console.WriteLine($"Database: {config.GetConnectionString("DefaultConnection")}");
Console.WriteLine($"JWT Issuer: {config["Jwt:Issuer"]}");
```

### Test Configuration
```bash
# Test different environments
ASPNETCORE_ENVIRONMENT=Development dotnet run
ASPNETCORE_ENVIRONMENT=Staging dotnet run  
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

## 🚨 Troubleshooting

### Common Issues

1. **Configuration Not Loading**
   - Check environment variable is set correctly
   - Verify JSON syntax in configuration files
   - Ensure file exists for the specified environment

2. **Database Connection Errors**
   - Verify connection string format
   - Check database server accessibility
   - Validate credentials

3. **JWT Token Issues**
   - Ensure keys are at least 256 bits
   - Check issuer/audience match across services
   - Verify token expiration settings

4. **Azure Service Connection Problems**
   - Validate Azure resource connection strings
   - Check access permissions
   - Verify resource exists in correct environment

## 📝 Best Practices

1. **Never commit sensitive data** - Use environment variables
2. **Test configuration changes** in Development first
3. **Use Azure Key Vault** for production secrets
4. **Monitor configuration drift** between environments
5. **Document configuration changes** in release notes
6. **Validate configurations** during deployment
