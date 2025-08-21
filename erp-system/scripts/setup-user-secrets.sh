#!/bin/bash

# Script to initialize user secrets for all ERP services
# This script sets up basic user secrets for local development

echo "🔐 Setting up user secrets for ERP services..."

# Function to initialize user secrets for a service
init_service_secrets() {
    local service_path=$1
    local service_name=$2
    local db_name=$3
    
    echo "📦 Initializing secrets for $service_name..."
    
    # Initialize user secrets if not already done
    dotnet user-secrets init --project "$service_path" || true
    
    # Set common secrets
    dotnet user-secrets set --project "$service_path" "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=${db_name};Trusted_Connection=true;MultipleActiveResultSets=true"
    dotnet user-secrets set --project "$service_path" "ConnectionStrings:Redis" "localhost:6379"
    dotnet user-secrets set --project "$service_path" "Jwt:Key" "your-super-secret-development-key-that-is-at-least-256-bits-long-for-jwt-signing"
    dotnet user-secrets set --project "$service_path" "Jwt:Issuer" "https://localhost:5000"
    dotnet user-secrets set --project "$service_path" "Jwt:Audience" "erp-system-dev"
    
    echo "✅ $service_name secrets configured"
}

# Initialize secrets for each service
init_service_secrets "erp-system/src/HRService/HRService.API" "HRService" "HRServiceDb_Dev"
init_service_secrets "erp-system/src/AuditService/AuditService.API" "AuditService" "AuditServiceDb_Dev"
init_service_secrets "erp-system/src/AIAgentService/AIAgentService.API" "AIAgentService" "AIAgentDB_Dev"
init_service_secrets "erp-system/gateway/self-hosted-gateway/ocelot" "Gateway" "GatewayDb_Dev"

# Set AIAgent-specific secrets
echo "🤖 Setting AIAgent-specific secrets..."
dotnet user-secrets set --project "erp-system/src/AIAgentService/AIAgentService.API" "AzureOpenAI:Endpoint" "https://your-dev-openai-resource.openai.azure.com/"
dotnet user-secrets set --project "erp-system/src/AIAgentService/AIAgentService.API" "AzureOpenAI:ApiKey" "your-azure-openai-api-key-here"
dotnet user-secrets set --project "erp-system/src/AIAgentService/AIAgentService.API" "AzureOpenAI:DeploymentName" "gpt-4"

echo ""
echo "🎉 User secrets setup complete!"
echo ""
echo "📝 Next steps:"
echo "1. Update the API keys and connection strings with your actual values"
echo "2. Ensure SQL Server LocalDB is running: SqlLocalDB start mssqllocaldb"
echo "3. Start Redis if using caching: docker run -d -p 6379:6379 redis:alpine"
echo ""
echo "🔍 To view secrets for a service:"
echo "   dotnet user-secrets list --project erp-system/src/HRService/HRService.API"
echo ""
echo "✏️  To modify a secret:"
echo "   dotnet user-secrets set --project [PROJECT_PATH] \"SecretKey\" \"SecretValue\""