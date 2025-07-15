# ERP Management System - Microservices Architecture

A comprehensive, cloud-native ERP Management System built with microservices architecture using a polyglot technology stack, designed for scalability, maintainability, and modern cloud deployment.

## 🏗️ Architecture Overview

This ERP system follows Domain-Driven Design (DDD) principles and microservices architecture patterns with the following key services:

### Core Business Services (.NET 10)
- **HR Service** - Employee management, payroll, and human resources
- **Inventory Service** - Stock management, warehousing, and supply chain
- **Accounting Service** - Financial management, invoicing, and reporting

### Supporting Services
- **Workflow Service** (Java) - Business process automation and workflow management
- **Notification Service** (Node.js) - Real-time notifications and communication

## 🛠️ Technology Stack

### Primary Technologies
- **.NET 10 (C#)** - Core business services with DDD architecture
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
│   ├── /WorkflowService         # Java service (placeholder)
│   └── /NotificationService     # Node.js service (placeholder)
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
- .NET 10 SDK
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

2. **Start local infrastructure**
   ```bash
   docker-compose up -d
   ```

3. **Run services locally**
   ```bash
   # HR Service
   cd src/HRService
   dotnet run

   # Inventory Service
   cd src/InventoryService
   dotnet run

   # Accounting Service
   cd src/AccountingService
   dotnet run
   ```

4. **Run tests**
   ```bash
   # Unit tests
   dotnet test tests/unit/

   # Integration tests
   dotnet test tests/integration/

   # End-to-end tests
   dotnet test tests/e2e/
   ```

## 🔧 Configuration

### Environment Variables
- `AZURE_CONNECTION_STRING` - Azure Service Bus connection
- `AZURE_KEY_VAULT_URL` - Azure Key Vault URL
- `DATABASE_CONNECTION_STRING` - Database connection string
- `ASPNETCORE_ENVIRONMENT` - Environment (Development, Staging, Production)

### Service Endpoints
- HR Service: `https://api.erp-system.com/v1/hr`
- Inventory Service: `https://api.erp-system.com/v1/inventory`
- Accounting Service: `https://api.erp-system.com/v1/accounting`

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
- [API Specifications](./docs/api-specs/)
- [Decision Records](./docs/decision-records/)
- [Service READMEs](./src/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the [documentation](./docs/)
- Review [decision records](./docs/decision-records/)
