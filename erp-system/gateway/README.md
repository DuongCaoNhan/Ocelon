# ERP System API Gateway

This directory contains the API Gateway implementation for the Ocelon ERP Management System, providing a unified entry point for all microservices.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Applications                     │
│  Web Portal │ Admin Panel │ Mobile App │ External Systems  │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                    API Gateway                              │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │ Authentication│ │Rate Limiting│ │Load Balancer│            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │   Routing   │ │  Monitoring │ │    CORS     │            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                 Microservices                               │
│ HR Service │Inventory│Accounting│Workflow│Notification       │
│  (.NET)    │ (.NET)  │ (.NET)   │ (Java) │  (Node.js)       │
└─────────────────────────────────────────────────────────────┘
```

## Deployment Options

### 1. Azure API Management (Production)
- **Path**: `./api-management/`
- **Use Case**: Production deployments
- **Features**: Enterprise-grade features, Azure integration
- **Documentation**: [Azure APIM Setup Guide](./api-management/README.md)

### 2. Self-Hosted Gateway (Development/Testing)
- **Path**: `./self-hosted-gateway/`
- **Use Case**: Local development, testing, on-premises
- **Options**: YARP or Ocelot
- **Documentation**: [Self-Hosted Setup Guide](./self-hosted-gateway/README.md)

## Features

### 🔐 Security
- **Authentication**: JWT Bearer tokens, API Keys
- **Authorization**: Role-based access control (RBAC)
- **OAuth 2.0**: Azure AD integration
- **Rate Limiting**: Per-user and per-service limits
- **CORS**: Cross-origin request management

### 🚀 Performance
- **Load Balancing**: Round-robin, weighted routing
- **Caching**: Response caching for GET requests
- **Compression**: Gzip response compression
- **Connection Pooling**: Optimized backend connections

### 📊 Observability
- **Logging**: Structured logging with correlation IDs
- **Metrics**: Request/response metrics
- **Tracing**: Distributed tracing support
- **Health Checks**: Service health monitoring
- **Application Insights**: Azure monitoring integration

### 🔄 Reliability
- **Circuit Breaker**: Fault tolerance patterns
- **Retry Logic**: Automatic retry with backoff
- **Timeout Management**: Request timeout handling
- **Graceful Degradation**: Service failure handling

## API Versioning Strategy

### URL Path Versioning
```
https://api.erp-system.com/v1/hr/employees
https://api.erp-system.com/v2/accounting/invoices
```

### Version Mapping
| Version | HR Service | Inventory | Accounting | Workflow | Notification |
|---------|------------|-----------|------------|----------|--------------|
| v1      | ✅ Current  | ✅ Current | ✅ Current  | ✅ Current| ✅ Current   |
| v2      | 🚧 Planned  | 🚧 Planned | 🚧 Planned  | 🚧 Planned| 🚧 Planned   |

## Service Routes

### Core Business Services
| Service | Route | Backend Port | Authentication |
|---------|-------|--------------|----------------|
| HR Service | `/v1/hr/**` | 5001 | Required |
| Inventory Service | `/v1/inventory/**` | 5002 | Required |
| Accounting Service | `/v1/accounting/**` | 5003 | Required |
| Workflow Service | `/v1/workflow/**` | 8080 | Required |
| Notification Service | `/v1/notifications/**` | 3000 | Required |

### System Routes
| Route | Purpose | Authentication |
|-------|---------|----------------|
| `/health` | System health check | None |
| `/health/{service}` | Service-specific health | None |
| `/gateway/info` | Gateway information | None |
| `/gateway/routes` | Available routes | Admin only |
| `/docs` | API documentation | None |

## Authentication & Authorization

### JWT Token Structure
```json
{
  "sub": "user-id-12345",
  "email": "user@company.com",
  "roles": ["HR.Employee", "Inventory.User"],
  "departments": ["HR", "Finance"],
  "iss": "https://login.microsoftonline.com/tenant-id",
  "aud": "api://erp-system",
  "exp": 1699123456
}
```

### Role-Based Access Control

#### HR Service Roles
- `HR.Employee`: Basic HR operations
- `HR.Manager`: Full HR management
- `HR.Admin`: System administration

#### Cross-Service Roles
- `Admin`: Full system access
- `Service.Account`: Service-to-service communication

### API Key Authentication
For service-to-service communication:
```bash
curl -H "X-API-Key: service-key-12345" \
     https://api.erp-system.com/v1/hr/employees
```

## Rate Limiting

### Default Limits
| User Type | Requests/Minute | Burst |
|-----------|-----------------|-------|
| Regular User | 100 | 10 |
| Service Account | 1000 | 100 |
| Admin | 500 | 50 |

### Service-Specific Limits
| Service | Limit | Reason |
|---------|-------|--------|
| HR Service | 50/min | Sensitive PII data |
| Notification Service | 200/min | High-frequency usage |
| Accounting Service | 75/min | Financial data protection |

## Environment Configuration

### Development
```bash
# Local development with self-hosted gateway
HR_SERVICE_URL=http://localhost:5001
INVENTORY_SERVICE_URL=http://localhost:5002
ACCOUNTING_SERVICE_URL=http://localhost:5003
WORKFLOW_SERVICE_URL=http://localhost:8080
NOTIFICATION_SERVICE_URL=http://localhost:3000
```

### Staging
```bash
# Kubernetes staging environment
HR_SERVICE_URL=http://hr-service-staging.default.svc.cluster.local
INVENTORY_SERVICE_URL=http://inventory-service-staging.default.svc.cluster.local
# ... other services
```

### Production
```bash
# Production Azure APIM or Kubernetes
GATEWAY_URL=https://api.erp-system.com
AZURE_APIM_INSTANCE=ocelon-erp-apim
```

## Quick Start

### 1. Development Setup (YARP Gateway)
```bash
# Clone repository
git clone https://github.com/DuongCaoNhan/Ocelon.git
cd Ocelon/erp-system/gateway/self-hosted-gateway/yarp

# Install dependencies
dotnet restore

# Configure settings
cp appsettings.json appsettings.Development.json
# Edit appsettings.Development.json with your settings

# Run gateway
dotnet run
```

### 2. Production Setup (Azure APIM)
```bash
# Deploy infrastructure
cd ../../../infrastructure/bicep
az deployment group create \
  --resource-group ocelon-erp-rg \
  --template-file main.bicep

# Import API definitions
cd ../../gateway/api-management
./deploy-apis.sh
```

## Testing

### Health Check
```bash
curl http://localhost:5000/health
```

### Service Access
```bash
# Get JWT token (replace with your auth endpoint)
TOKEN=$(curl -X POST https://your-auth-server/token \
  -d "grant_type=client_credentials" \
  -d "client_id=your-client-id" \
  -d "client_secret=your-secret" | jq -r '.access_token')

# Test HR service
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/v1/hr/employees

# Test with API key
curl -H "X-API-Key: dev-service-key-12345" \
     http://localhost:5000/v1/inventory/products
```

## Monitoring & Observability

### Application Insights Dashboard
- Request volume and latency
- Error rates by service
- Authentication failures
- Rate limiting violations

### Custom Metrics
- Gateway throughput
- Backend service latency
- Authentication success rate
- Cache hit ratio

### Log Correlation
Each request includes a correlation ID for tracing across services:
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "INFO",
  "message": "Request processed",
  "correlationId": "abc123-def456-ghi789",
  "service": "api-gateway",
  "method": "GET",
  "path": "/v1/hr/employees",
  "statusCode": 200,
  "duration": 45
}
```

## Security Best Practices

### Network Security
- TLS 1.3 encryption in transit
- Private backend communication
- Network segmentation
- IP allowlisting for admin endpoints

### Data Protection
- PII data masking in logs
- Sensitive field redaction
- GDPR compliance headers
- Audit trail for all operations

### Vulnerability Management
- Regular security scanning
- Dependency updates
- Penetration testing
- Security headers (HSTS, CSP, etc.)

## CI/CD Pipeline

### GitHub Actions Workflow
```yaml
name: Deploy API Gateway
on:
  push:
    branches: [main]
    paths: ['gateway/**']

jobs:
  deploy-apim:
    if: contains(github.event.head_commit.modified, 'api-management/')
    runs-on: ubuntu-latest
    steps:
    - name: Deploy to Azure APIM
      run: ./gateway/api-management/deploy-apis.sh

  deploy-self-hosted:
    if: contains(github.event.head_commit.modified, 'self-hosted-gateway/')
    runs-on: ubuntu-latest
    steps:
    - name: Build and deploy gateway
      run: ./gateway/self-hosted-gateway/deploy.sh
```

## Troubleshooting

### Common Issues

1. **503 Service Unavailable**
   - Check backend service health
   - Verify network connectivity
   - Review rate limiting settings

2. **401 Unauthorized**
   - Validate JWT token expiration
   - Check token claims and roles
   - Verify audience and issuer

3. **429 Too Many Requests**
   - Review rate limiting configuration
   - Check user-specific limits
   - Implement exponential backoff

### Debug Mode
```bash
# Enable debug logging
export ASPNETCORE_ENVIRONMENT=Development
export Logging__LogLevel__Default=Debug

# Check gateway routes
curl http://localhost:5000/gateway/routes

# Monitor real-time logs
tail -f gateway.log | grep ERROR
```

## Performance Tuning

### Connection Optimization
```json
{
  "HttpClientOptions": {
    "MaxConnectionsPerServer": 50,
    "PooledConnectionLifetime": "00:10:00",
    "PooledConnectionIdleTimeout": "00:02:00"
  }
}
```

### Caching Strategy
- GET requests: 5-minute cache
- Reference data: 1-hour cache
- User-specific data: No cache
- Public endpoints: 15-minute cache

## Support & Documentation

- **API Documentation**: Available at `/docs` endpoint
- **OpenAPI Specs**: `./api-management/openapi-specs/`
- **Architecture Docs**: `../docs/architecture.md`
- **Support**: Create GitHub issue or contact dev team

---

Built with ❤️ for the Ocelon ERP Management System
