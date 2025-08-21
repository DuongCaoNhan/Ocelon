# PowerShell script to initialize user secrets for all ERP services
# This script sets up basic user secrets for local development

Write-Host "🔐 Setting up user secrets for ERP services..." -ForegroundColor Green

# Function to initialize user secrets for a service
function Init-ServiceSecrets {
    param(
        [string]$ServicePath,
        [string]$ServiceName,
        [string]$DbName
    )
    
    Write-Host "📦 Initializing secrets for $ServiceName..." -ForegroundColor Yellow
    
    # Initialize user secrets if not already done
    dotnet user-secrets init --project $ServicePath
    
    # Set common secrets
    dotnet user-secrets set --project $ServicePath "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=$DbName;Trusted_Connection=true;MultipleActiveResultSets=true"
    dotnet user-secrets set --project $ServicePath "ConnectionStrings:Redis" "localhost:6379"
    dotnet user-secrets set --project $ServicePath "Jwt:Key" "your-super-secret-development-key-that-is-at-least-256-bits-long-for-jwt-signing"
    dotnet user-secrets set --project $ServicePath "Jwt:Issuer" "https://localhost:5000"
    dotnet user-secrets set --project $ServicePath "Jwt:Audience" "erp-system-dev"
    
    Write-Host "✅ $ServiceName secrets configured" -ForegroundColor Green
}

# Initialize secrets for each service
Init-ServiceSecrets "erp-system\src\HRService\HRService.API" "HRService" "HRServiceDb_Dev"
Init-ServiceSecrets "erp-system\src\AuditService\AuditService.API" "AuditService" "AuditServiceDb_Dev"
Init-ServiceSecrets "erp-system\src\AIAgentService\AIAgentService.API" "AIAgentService" "AIAgentDB_Dev"
Init-ServiceSecrets "erp-system\gateway\self-hosted-gateway\ocelot" "Gateway" "GatewayDb_Dev"

# Set AIAgent-specific secrets
Write-Host "🤖 Setting AIAgent-specific secrets..." -ForegroundColor Yellow
dotnet user-secrets set --project "erp-system\src\AIAgentService\AIAgentService.API" "AzureOpenAI:Endpoint" "https://your-dev-openai-resource.openai.azure.com/"
dotnet user-secrets set --project "erp-system\src\AIAgentService\AIAgentService.API" "AzureOpenAI:ApiKey" "your-azure-openai-api-key-here"
dotnet user-secrets set --project "erp-system\src\AIAgentService\AIAgentService.API" "AzureOpenAI:DeploymentName" "gpt-4"

Write-Host ""
Write-Host "🎉 User secrets setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Next steps:" -ForegroundColor Cyan
Write-Host "1. Update the API keys and connection strings with your actual values"
Write-Host "2. Ensure SQL Server LocalDB is running: SqlLocalDB start mssqllocaldb"
Write-Host "3. Start Redis if using caching: docker run -d -p 6379:6379 redis:alpine"
Write-Host ""
Write-Host "🔍 To view secrets for a service:" -ForegroundColor Cyan
Write-Host "   dotnet user-secrets list --project erp-system\src\HRService\HRService.API"
Write-Host ""
Write-Host "✏️  To modify a secret:" -ForegroundColor Cyan
Write-Host "   dotnet user-secrets set --project [PROJECT_PATH] `"SecretKey`" `"SecretValue`""