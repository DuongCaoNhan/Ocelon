# ERP Management System - Microservices Architecture

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Environment](https://img.shields.io/badge/environment-development%20%7C%20staging%20%7C%20production-blue)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)]()
[![Gateway](https://img.shields.io/badge/gateway-ocelot%20%7C%20yarp-orange)]()

A comprehensive, cloud-native ERP Management System built with microservices architecture using a polyglot technology stack, designed for scalability, maintainability, and modern cloud deployment with full environment-specific configuration support.

## 🏗️ Architecture Overview

This ERP system follows Domain-Driven Design (DDD) principles and microservices architecture patterns with the following key services:

### Core Business Services (.NET 8)
- **HR Service** - Employee management, payroll, and human resources
- **Inventory Service** - Stock management, warehousing, and supply chain
- **Accounting Service** - Financial management, invoicing, and reporting

### Supporting Services
- **Workflow Service** (Java) - Business process automation and workflow management
- **Notification Service** (Node.js) - Real-time notifications and communication
- **Audit Service** (.NET) - System-wide audit logging and compliance tracking
- **AI Agent Service** (.NET) - AI-powered automation and intelligent assistance

### Gateway Services
- **Ocelot Gateway** - .NET API Gateway with routing, authentication, and rate limiting
- **YARP Gateway** - Alternative high-performance reverse proxy gateway

## 🛠️ Technology Stack

### Primary Technologies
- **.NET 8 (C#)** - Core business services with DDD architecture
- **Java** - Workflow automation services
- **Node.js** - Real-time notification services
- **Azure Cloud Platform** - Complete cloud infrastructure

### Cloud Infrastructure
- **Azure Kubernetes Service (AKS)** - Container orchestration
- **Azure Service Bus/Event Grid** - Asynchronous messaging
- **Azure API Management** - API gateway with versioning
- **Azure Key Vault** - Secure secret management
- **Azure Monitor/Application Insights** - Centralized logging and monitoring

### Testing Framework
- **xUnit** - Unit, integration, and end-to-end testing
- **Testcontainers** - Integration testing with real dependencies

## 📁 Project Structure

```
/erp-system
├── /src                          # Source code for all services
│   ├── /HRService               # .NET DDD: Domain, Application, Infrastructure, API
│   ├── /InventoryService        # .NET DDD architecture
│   ├── /AccountingService       # .NET DDD architecture
│   ├── /AuditService            # .NET DDD: System audit and compliance
│   ├── /AIAgentService          # .NET: AI-powered automation
│   ├── /WorkflowService         # Java service (placeholder)
│   └── /NotificationService     # Node.js service (placeholder)
├── /gateway                     # API Gateway services
│   ├── /self-hosted-gateway     # Self-hosted gateway implementations
│   │   ├── /ocelot             # Ocelot .NET API Gateway
│   │   └── /yarp               # YARP .NET Gateway
│   ├── /api-management         # Azure API Management configurations
│   └── /infrastructure         # Gateway infrastructure as code
├── /tests                       # Comprehensive testing suite
│   ├── /unit                    # Unit tests per service
│   ├── /integration             # Integration tests
│   └── /e2e                     # End-to-end tests
├── /config                      # Environment-specific configurations
│   ├── /dev                     # Development environment
│   ├── /staging                 # Staging environment
│   └── /prod                    # Production environment
├── /infrastructure              # Infrastructure as Code
│   ├── /bicep                   # Azure Bicep templates
│   ├── /terraform               # Terraform configurations
│   ├── /helm                    # Helm charts for AKS
│   └── /service-discovery       # Service discovery configurations
├── /database                    # Database management
│   ├── /schemas                 # Database schemas per service
│   └── /migrations              # Database migration scripts
├── /deploy                      # CI/CD pipeline configurations
│   ├── /github-actions          # GitHub Actions workflows
│   └── /azure-devops            # Azure DevOps pipelines
└── /docs                        # Documentation
    ├── architecture.md          # System architecture documentation
    ├── /api-specs              # OpenAPI specifications
    └── /decision-records       # Architecture Decision Records
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- Azure CLI
- kubectl
- Helm

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd erp-system
   ```

2. **Set up environment configuration**
   ```bash
   # Copy environment template
   cp .env.template .env
   # Edit .env with your configuration values
   
   # Set environment (Development is default)
   set ASPNETCORE_ENVIRONMENT=Development
   ```

3. **Start local infrastructure**
   ```bash
   docker-compose up -d
   ```

4. **Build and run services**
   ```bash
   # Build entire solution
   dotnet build ERP.sln
   
   # Run specific services
   cd src/HRService/HRService.API
   dotnet run
   
   # Run gateway
   cd gateway/self-hosted-gateway/ocelot
   dotnet run
   ```

5. **Access services**
   - Gateway: `http://localhost:5000`
   - Gateway Swagger: `http://localhost:5000/swagger`
   - HR Service: `http://localhost:5001` 
   - Inventory Service: `http://localhost:5002`
   - Accounting Service: `http://localhost:5003`

6. **Run tests**
   ```bash
   # Unit tests
   dotnet test tests/unit/

   # Integration tests
   dotnet test tests/integration/

   # End-to-end tests
   dotnet test tests/e2e/
   ```

## 🔧 Configuration & Environment Management

### Environment-Specific Configurations
The system supports multiple deployment environments with dedicated configurations:

- **Development** - Local development with debugging enabled
- **Staging** - Pre-production testing environment  
- **Production** - Live production environment

### Configuration Files Structure
```
├── /gateway/self-hosted-gateway/ocelot/
│   ├── ocelot.Development.json      # Development gateway config
│   ├── ocelot.Staging.json          # Staging gateway config
│   ├── ocelot.Production.json       # Production gateway config
│   └── ocelot.local.json           # Local overrides (gitignored)
├── /src/[ServiceName]/[ServiceName].API/
│   ├── appsettings.json             # Base configuration
│   ├── appsettings.Development.json # Development overrides
│   ├── appsettings.Staging.json     # Staging overrides
│   └── appsettings.Production.json  # Production overrides
└── .env.template                    # Environment variables template
```

### Key Environment Differences

| Configuration | Development | Staging | Production |
|---------------|-------------|---------|------------|
| **Database** | LocalDB/Local SQL | Azure SQL (Staging) | Azure SQL (Production) |
| **Logging Level** | Debug | Information | Warning |
| **JWT Expiry** | 60 minutes | 30 minutes | 15 minutes |
| **Swagger** | ✅ Enabled | ✅ Enabled | ❌ Disabled |
| **Rate Limiting** | ❌ Disabled | ✅ Moderate | ✅ Strict |
| **Caching TTL** | 5 minutes | 15-20 minutes | 30-60 minutes |
| **HTTPS** | Optional | Required | Required |

### Environment Setup
1. **Copy environment template:**
   ```bash
   cp .env.template .env
   ```

2. **Set environment variable:**
   ```bash
   # Windows
   set ASPNETCORE_ENVIRONMENT=Development
   set ASPNETCORE_ENVIRONMENT=Staging
   set ASPNETCORE_ENVIRONMENT=Production
   
   # Linux/macOS
   export ASPNETCORE_ENVIRONMENT=Development
   ```

3. **Run with specific environment:**
   ```bash
   cd src/HRService/HRService.API
   dotnet run --environment Production
   ```

For detailed configuration documentation, see [Environment Configuration Guide](./docs/DOTNET-ENVIRONMENT-CONFIG.md).

### Service Endpoints

| Service | Development | Staging | Production |
|---------|-------------|---------|------------|
| **Gateway** | `http://localhost:5000` | `https://api-staging.ocelon.com` | `https://api.ocelon.com` |
| **HR Service** | `http://localhost:5001` | `https://hr-service-staging.ocelon.com` | `https://hr-service.ocelon.com` |
| **Inventory Service** | `http://localhost:5002` | `https://inventory-service-staging.ocelon.com` | `https://inventory-service.ocelon.com` |
| **Accounting Service** | `http://localhost:5003` | `https://accounting-service-staging.ocelon.com` | `https://accounting-service.ocelon.com` |
| **Audit Service** | `http://localhost:5004` | `https://audit-service-staging.ocelon.com` | `https://audit-service.ocelon.com` |
| **AI Agent Service** | `http://localhost:5005` | `https://ai-service-staging.ocelon.com` | `https://ai-service.ocelon.com` |

### Gateway Routes
- HR API: `/v1/hr/*` → HR Service
- Inventory API: `/v1/inventory/*` → Inventory Service  
- Accounting API: `/v1/accounting/*` → Accounting Service
- Health Checks: `/health/all` → Aggregated health status

## 🔐 Security

- **mTLS** - Mutual TLS for inter-service communication
- **Azure Key Vault** - Centralized secret management
- **JWT Tokens** - API authentication and authorization
- **Service Mesh** - Network security with Istio/Linkerd

## 📊 Monitoring & Observability

- **Azure Monitor** - Centralized logging and metrics
- **Application Insights** - Application performance monitoring
- **Structured Logging** - Consistent logging across all services
- **Health Checks** - Service health monitoring

## 🚀 Deployment

### Azure Kubernetes Service (AKS)
```bash
# Deploy to AKS using Helm
helm install erp-system ./infrastructure/helm/erp-system
```

### CI/CD Pipeline
- **GitHub Actions** - Automated testing and deployment
- **Azure DevOps** - Enterprise CI/CD pipeline
- **GitOps** - Infrastructure and application deployment

## 📚 Documentation

- [Architecture Documentation](./docs/architecture.md)
- [Environment Configuration Guide](./docs/DOTNET-ENVIRONMENT-CONFIG.md)
- [API Specifications](./docs/api-specs/)
- [Gateway Configuration](./gateway/self-hosted-gateway/ocelot/README-Environment-Config.md)
- [Service READMEs](./src/)

### Configuration Files
- [Environment Variables Template](./.env.template)
- [Ocelot Gateway Configs](./gateway/self-hosted-gateway/ocelot/)
- [Service-specific Configs](./src/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support & Troubleshooting

### Common Environment Configuration Issues

#### Configuration Not Loading
```bash
# Check environment variable
echo $ASPNETCORE_ENVIRONMENT  # Linux/macOS
echo %ASPNETCORE_ENVIRONMENT%  # Windows

# Verify configuration files exist
ls src/HRService/HRService.API/appsettings.*.json
ls gateway/self-hosted-gateway/ocelot/ocelot.*.json
```

#### Service Connection Issues
```bash
# Test service health
curl http://localhost:5001/health  # HR Service
curl http://localhost:5002/health  # Inventory Service
curl http://localhost:5000/health/all  # Gateway health aggregate
```

#### Database Connection Problems
- Verify connection strings in `appsettings.{Environment}.json`
- Check database server accessibility
- Validate credentials and permissions

For detailed troubleshooting, see [Environment Configuration Guide](./docs/DOTNET-ENVIRONMENT-CONFIG.md#troubleshooting)

### Support Channels
For support and questions:
- Create an issue in the repository
- Check the [documentation](./docs/)
- Review environment-specific configurations
