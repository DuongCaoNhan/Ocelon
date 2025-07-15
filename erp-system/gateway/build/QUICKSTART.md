# Quick Start - Build API Gateway

## For Windows (PowerShell)

```powershell
# Navigate to the gateway directory
cd "c:\Users\Admin\source\repos\Ocelon\erp-system\gateway"

# Run the build script for development environment
.\build\build.ps1 -Environment dev -BuildSelfHosted -RunTests

# For production deployment
.\build\build.ps1 -Environment prod -ResourceGroup "ocelon-prod-rg"
```

## For Linux/macOS (Bash)

```bash
# Navigate to the gateway directory
cd /path/to/Ocelon/erp-system/gateway

# Make script executable (first time only)
chmod +x build/build.sh

# Run the build script for development environment
./build/build.sh -e dev -b -t

# For production deployment
./build/build.sh -e prod -r "ocelon-prod-rg"
```

## What the Build Script Does

1. **Validates Prerequisites** - Checks Azure CLI, .NET SDK, and login status
2. **Creates Resource Group** - Sets up Azure resource group if needed
3. **Deploys Infrastructure** - Uses Bicep templates to deploy APIM and supporting services
4. **Configures Policies** - Imports security and rate limiting policies
5. **Builds Self-Hosted Gateways** - Compiles YARP and Ocelot gateways (optional)
6. **Runs Tests** - Validates deployment with basic connectivity tests (optional)
7. **Generates Documentation** - Creates deployment summary and next steps

## Expected Output

```
🚀 Building Ocelon ERP API Gateway for dev environment
🔍 Validating prerequisites...
✅ Azure CLI version: 2.x.x
✅ Logged in as: your-account@domain.com
✅ .NET SDK version: 8.x.x
🏗️ Setting up resource group...
✅ Resource group created
☁️ Deploying Azure infrastructure...
✅ Templates validated successfully
✅ Infrastructure deployed successfully
📝 Configuring API Management policies...
✅ Global policy imported
✅ Policy imported for hr-api
🔨 Building self-hosted gateways...
✅ YARP gateway built successfully
✅ Ocelot gateway built successfully
🧪 Running gateway tests...
✅ APIM health check passed
✅ Authentication test passed
📋 Generating deployment summary...
🎉 API Gateway build completed successfully!
```

## After Build Completion

Check the generated files:
- `build/deployment-outputs-{env}.json` - Contains Azure resource details
- `build/deployment-summary-{env}.md` - Human-readable deployment summary

## Next Steps

1. Update backend service URLs in APIM named values
2. Configure JWT signing key in Key Vault
3. Create API subscriptions for your applications
4. Test your APIs with the provided endpoints

## Troubleshooting

If you encounter issues:
1. Ensure you're logged into Azure CLI: `az login`
2. Verify you have sufficient permissions on the subscription
3. Check the Azure region supports API Management service
4. Review the deployment summary for specific error messages
