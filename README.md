# Ocelon ERP Management System

A comprehensive microservices-based Enterprise Resource Planning (ERP) system built with modern technologies and polyglot architecture.

## 🏗️ Architecture Overview

This ERP system follows a **microservices architecture** with **Domain-Driven Design (DDD)** principles, implementing a **polyglot** approach using multiple programming languages and frameworks.

### 🛠️ Technology Stack

| Service | Technology | Framework | Database |
|---------|------------|-----------|----------|
| **HR Service** | .NET 8 | ASP.NET Core Web API | SQL Server |
| **Inventory Service** | .NET 8 | ASP.NET Core Web API | SQL Server |
| **Accounting Service** | .NET 8 | ASP.NET Core Web API | SQL Server |
| **Workflow Service** | Java 21 | Spring Boot 3.2 | PostgreSQL |
| **Notification Service** | Node.js 18 | Express.js + Socket.IO | MongoDB |

### 🏛️ System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway (Azure APIM)                │
├─────────────────────────────────────────────────────────────┤
│  Service Mesh (Istio) + Load Balancer + Authentication     │
├─────────────────────────────────────────────────────────────┤
│ HR Service  │ Inventory  │ Accounting │ Workflow │ Notify  │
│   (.NET)    │  Service   │  Service   │ Service  │ Service │
│             │   (.NET)   │   (.NET)   │  (Java)  │ (Node)  │
├─────────────────────────────────────────────────────────────┤
│         Message Bus (Azure Service Bus + Events)           │
├─────────────────────────────────────────────────────────────┤
│  SQL Server │ SQL Server │ SQL Server │PostgreSQL│ MongoDB │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Project Structure

```
ocelon/
├── erp-system/
│   ├── src/
│   │   ├── HRService/                    ✅ Complete Implementation
│   │   │   ├── HRService.Domain/
│   │   │   ├── HRService.Application/
│   │   │   ├── HRService.Infrastructure/
│   │   │   └── HRService.API/
│   │   ├── InventoryService/             📁 Ready for Implementation
│   │   ├── AccountingService/            📁 Ready for Implementation
│   │   ├── WorkflowService/              ⚙️ Java Spring Boot
│   │   └── NotificationService/          ⚙️ Node.js Express
│   ├── tests/
│   │   ├── unit/                         ✅ xUnit + Jest
│   │   └── integration/                  📋 Ready
│   ├── infrastructure/
│   │   ├── azure/                        ☁️ Bicep Templates
│   │   ├── kubernetes/                   🐳 K8s Manifests
│   │   └── terraform/                    🏗️ IaC Scripts
│   ├── ci-cd/
│   │   └── github-actions/               🔄 DevOps Pipelines
│   └── docs/                             📚 Documentation
├── ERP.sln                               ✅ Solution File
└── README.md                             📖 This File
```

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Java 21** - [Download](https://openjdk.org/projects/jdk/21/)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Azure CLI** - [Download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

### 🏃‍♂️ Running the Services

#### 1. HR Service (.NET)
```bash
cd erp-system/src/HRService/HRService.API
dotnet restore
dotnet build
dotnet run
```
**Endpoints**: `https://localhost:7001/api/employees`

#### 2. Notification Service (Node.js)
```bash
cd erp-system/src/NotificationService
npm install
npm start
```
**Endpoints**: `http://localhost:3000/api/notifications`

#### 3. Workflow Service (Java)
```bash
cd erp-system/src/WorkflowService
mvn clean install
mvn spring-boot:run
```
**Endpoints**: `http://localhost:8080/api/workflows`

### 🐳 Docker Deployment

#### Build All Services
```bash
# Build HR Service
docker build -t ocelon/hr-service ./src/HRService

# Build Notification Service  
docker build -t ocelon/notification-service ./src/NotificationService

# Build Workflow Service
docker build -t ocelon/workflow-service ./src/WorkflowService
```

#### Run with Docker Compose
```bash
cd erp-system
docker-compose up -d
```

## 🏗️ Build & Test

### Build Entire Solution
```bash
cd erp-system
dotnet build ERP.sln
```

### Run Tests
```bash
# .NET Unit Tests
dotnet test tests/unit/HRService/

# Node.js Tests
cd src/NotificationService && npm test

# Java Tests  
cd src/WorkflowService && mvn test
```

### Build Status
- ✅ **HR Service**: Complete DDD implementation with CQRS
- ✅ **Solution Build**: All projects compile successfully 
- ✅ **Unit Tests**: xUnit framework configured
- ✅ **Integration**: Service Bus messaging ready

## 🌟 Features

### 👥 HR Service (.NET 8)
- **Employee Management**: CRUD operations with DDD patterns
- **Payroll Processing**: Automated salary calculations
- **Leave Management**: Request and approval workflows
- **Performance Tracking**: KPI monitoring and reviews

### 📦 Inventory Service (Planned)
- **Stock Management**: Real-time inventory tracking
- **Warehouse Operations**: Multi-location support
- **Supply Chain**: Vendor and procurement management
- **Reporting**: Analytics and forecasting

### 💰 Accounting Service (Planned)
- **Financial Records**: General ledger management
- **Invoicing**: Automated billing and payments
- **Tax Management**: Compliance and reporting
- **Financial Analytics**: Real-time dashboards

### ⚙️ Workflow Service (Java)
- **Business Process Management**: Flowable BPM integration
- **Approval Workflows**: Multi-step approval chains
- **Task Automation**: Scheduled and event-driven tasks
- **Process Analytics**: Performance monitoring

### 🔔 Notification Service (Node.js)
- **Real-time Messaging**: WebSocket support with Socket.IO
- **Email Notifications**: SMTP integration
- **SMS Alerts**: Third-party SMS providers
- **Push Notifications**: Mobile and web notifications

## ☁️ Cloud Infrastructure

### Azure Resources
- **AKS (Azure Kubernetes Service)**: Container orchestration
- **Azure Service Bus**: Message queuing and pub/sub
- **Azure SQL Database**: Relational data storage
- **Azure API Management**: API gateway and management
- **Azure Key Vault**: Secrets and configuration management
- **Application Insights**: Monitoring and telemetry

### Deployment Pipeline
```bash
# Infrastructure Deployment
cd infrastructure/azure
az deployment group create --template-file main.bicep

# Kubernetes Deployment
cd infrastructure/kubernetes
kubectl apply -f ./manifests/

# CI/CD Pipeline
# GitHub Actions automatically deploy on main branch push
```

## 🔧 Development

### Domain-Driven Design (DDD)
Each .NET service follows clean architecture:
- **Domain Layer**: Entities, Value Objects, Domain Services
- **Application Layer**: Use Cases, Commands, Queries (CQRS)
- **Infrastructure Layer**: Data Access, External Services
- **API Layer**: Controllers, DTOs, Validation

### CQRS + Event Sourcing
- **Commands**: Write operations with business validation
- **Queries**: Optimized read operations
- **Events**: Domain events for inter-service communication
- **Handlers**: MediatR pattern for decoupling

### Microservices Patterns
- **API Gateway**: Single entry point with routing
- **Service Discovery**: Automatic service registration
- **Circuit Breaker**: Fault tolerance and resilience
- **Event-Driven**: Asynchronous communication via messages

## 📊 Monitoring & Observability

- **Health Checks**: Built-in endpoint monitoring
- **Logging**: Structured logging with Serilog
- **Metrics**: Prometheus integration
- **Tracing**: Distributed tracing with OpenTelemetry
- **Dashboards**: Grafana visualization

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

## 📄 License

This project is licensed under the Apache 2.0 License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- **Documentation**: [Wiki](https://github.com/DuongCaoNhan/Ocelon/wiki)
- **Issues**: [GitHub Issues](https://github.com/DuongCaoNhan/Ocelon/issues)
- **Discussions**: [GitHub Discussions](https://github.com/DuongCaoNhan/Ocelon/discussions)

---

## 🎯 Project Status

| Component | Status |
|-----------|--------|
| HR Service | 🚧 In Development |
| Inventory Service | 📋 Planned |
| Accounting Service | 📋 Planned |
| Workflow Service | 🚧 In Development |
| Notification Service | 🚧 In Development |
| Infrastructure | 🚧 In Development |
| CI/CD Pipeline | 🚧 In Development |
| Documentation | 🚧 In Development |

Built with ❤️ using modern microservices architecture and best practices.