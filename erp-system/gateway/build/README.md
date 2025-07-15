# API Gateway Build Scripts

This directory contains build and deployment automation scripts for the Ocelon ERP API Gateway.

## Scripts

### `build.ps1` (Windows PowerShell)
Complete build and deployment script for Windows environments.

**Usage:**
```powershell
# Deploy to development environment
.\build.ps1 -Environment dev -BuildSelfHosted -RunTests

# Deploy to production
.\build.ps1 -Environment prod -ResourceGroup "ocelon-prod-rg"

# Skip infrastructure deployment (policies only)
.\build.ps1 -Environment staging -SkipInfrastructure
```

**Parameters:**
- `-Environment` (required): Target environment (dev, staging, prod)
- `-ResourceGroup`: Azure resource group name (default: ocelon-erp-rg)
- `-Location`: Azure region (default: East US)
- `-SkipInfrastructure`: Skip Bicep deployment
- `-BuildSelfHosted`: Build YARP and Ocelot gateways
- `-RunTests`: Execute post-deployment tests

### `build.sh` (Linux/macOS Bash)
Cross-platform build script for Unix-based systems.

**Usage:**
```bash
# Make script executable
chmod +x build.sh

# Deploy to development environment
./build.sh -e dev -b -t

# Deploy to production with custom settings
./build.sh -e prod -r "ocelon-prod-rg" -l "West US 2"

# Skip infrastructure deployment
./build.sh -e staging -s
```

**Parameters:**
- `-e, --environment` (required): Target environment
- `-r, --resource-group`: Azure resource group name
- `-l, --location`: Azure region
- `-s, --skip-infra`: Skip infrastructure deployment
- `-b, --build-self`: Build self-hosted gateways
- `-t, --run-tests`: Run tests after deployment
- `-h, --help`: Show help message

## Build Process

Both scripts follow the same build process:

### 1. Prerequisites Validation
- ✅ Azure CLI installation and login status
- ✅ .NET SDK (if building self-hosted gateways)
- ✅ Required permissions

### 2. Resource Group Setup
- Creates resource group if it doesn't exist
- Validates location and naming

### 3. Infrastructure Deployment
- Validates Bicep templates
- Deploys Azure API Management infrastructure
- Configures monitoring and security
- Saves deployment outputs

### 4. Policy Configuration
- Imports global APIM policies
- Configures API-specific policies
- Sets up security and rate limiting

### 5. Self-Hosted Gateway Build
- Restores .NET dependencies
- Builds YARP and Ocelot gateways
- Creates publish artifacts

### 6. Testing
- Health endpoint validation
- Authentication testing
- Basic connectivity tests

### 7. Documentation
- Generates deployment summary
- Creates configuration guide
- Provides next steps

## Output Files

The build process creates several output files:

```
build/
├── deployment-outputs-{env}.json     # Azure resource details
├── deployment-summary-{env}.md       # Human-readable summary
└── logs/                             # Build logs (if configured)
```

## Prerequisites

### Azure CLI
```bash
# Install Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login to Azure
az login
```

### .NET SDK (for self-hosted gateways)
```bash
# Install .NET 8 SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### PowerShell (for Windows script)
PowerShell 5.1+ or PowerShell Core 7+ required.

## Environment-Specific Configurations

### Development
- Developer SKU APIM
- Relaxed security policies
- Debug logging enabled
- Local backend services

### Staging
- Standard SKU APIM
- Production-like policies
- Limited monitoring
- Staging backend services

### Production
- Premium SKU APIM
- Full security policies
- Comprehensive monitoring
- Production backend services

## Troubleshooting

### Common Issues

1. **Azure CLI not logged in**
   ```bash
   az login
   az account set --subscription "your-subscription-id"
   ```

2. **Insufficient permissions**
   - Ensure you have Contributor role on the subscription
   - Verify resource group permissions

3. **Bicep template validation fails**
   - Check template syntax
   - Verify parameter values
   - Review Azure region availability

4. **Policy import failures**
   - Verify XML syntax
   - Check named value references
   - Ensure APIs exist before applying policies

5. **Self-hosted gateway build fails**
   - Verify .NET SDK installation
   - Check project file dependencies
   - Review build logs for specific errors

### Debug Mode

Add debug flags for verbose output:

**PowerShell:**
```powershell
$VerbosePreference = "Continue"
.\build.ps1 -Environment dev -Verbose
```

**Bash:**
```bash
set -x  # Enable debug mode
./build.sh -e dev
```

## Solution File Management

The gateway includes a Visual Studio solution file (`Gateway.sln`) that provides:

### ✅ Benefits
- **Unified Development**: Single workspace for all .NET components
- **IntelliSense & Navigation**: Full IDE support for code navigation
- **Integrated Testing**: Run tests directly from IDE
- **Dependency Management**: Clear project references and NuGet packages
- **Team Collaboration**: Familiar structure for .NET developers

### 📁 Solution Structure
```
Gateway.sln
├── src/
│   ├── Gateway.Shared/          # Common libraries and extensions
│   ├── Gateway.Tests/           # Unit tests
│   └── Gateway.IntegrationTests/ # Integration tests
├── self-hosted-gateway/
│   ├── yarp/                    # YARP gateway project
│   └── ocelot/                  # Ocelot gateway project
└── Solution Folders/
    ├── Infrastructure/          # Bicep templates
    ├── Configurations/          # APIM policies and configs
    ├── OpenAPI Specifications/  # API schemas
    ├── Build Scripts/           # Automation scripts
    └── Documentation/           # All documentation
```

### 🔧 Working with the Solution

**Open in Visual Studio:**
```bash
# Open the solution
start Gateway.sln
# or
code Gateway.sln  # VS Code with C# extension
```

**Build from command line:**
```bash
# Build all projects
dotnet build Gateway.sln

# Build specific configuration
dotnet build Gateway.sln --configuration Release

# Run tests
dotnet test Gateway.sln
```

**Environment-specific builds:**
```bash
# Development build
dotnet build Gateway.sln --configuration Development

# Production build
dotnet build Gateway.sln --configuration Production
```

## CI/CD Integration

### GitHub Actions
```yaml
- name: Deploy API Gateway
  run: |
    chmod +x gateway/build/build.sh
    ./gateway/build/build.sh -e ${{ env.ENVIRONMENT }} -b -t
```

### Azure DevOps
```yaml
- task: PowerShell@2
  displayName: 'Deploy API Gateway'
  inputs:
    filePath: 'gateway/build/build.ps1'
    arguments: '-Environment $(Environment) -BuildSelfHosted -RunTests'
```

## Next Steps

After successful build:

1. **Configure Backend Services**
   - Update named values with actual service URLs
   - Test connectivity to backend services

2. **Set Up Authentication**
   - Configure JWT signing keys
   - Set up Azure AD integration

3. **Create API Subscriptions**
   - Generate subscription keys for consumers
   - Configure product access

4. **Monitor and Scale**
   - Review Application Insights metrics
   - Configure auto-scaling policies
