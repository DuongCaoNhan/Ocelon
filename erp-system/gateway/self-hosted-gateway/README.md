# Self-Hosted API Gateway for ERP System

This directory contains self-hosted API Gateway implementations using YARP (Yet Another Reverse Proxy) and Ocelot for local development and testing environments.

## Overview

The self-hosted gateways provide:
- **Request routing** to backend microservices
- **Authentication and authorization** (JWT + API Key)
- **Rate limiting** and throttling
- **Load balancing** and health checks
- **CORS** policy management
- **Request/response transformation**
- **Monitoring and logging**

## Gateway Options

### 1. YARP Gateway (Recommended)
- **Technology**: Microsoft's YARP (Yet Another Reverse Proxy)
- **Performance**: High-performance, low-latency
- **Features**: Advanced routing, rate limiting, health checks
- **Configuration**: JSON-based configuration
- **Port**: 5000 (HTTP), 5001 (HTTPS)

### 2. Ocelot Gateway
- **Technology**: Ocelot API Gateway
- **Features**: Rich feature set, Swagger integration
- **Configuration**: JSON-based configuration with Consul support
- **Port**: 5000 (HTTP), 5001 (HTTPS)

## Quick Start

### Prerequisites

```bash
# Install .NET 8 SDK
dotnet --version  # Should be 8.0 or higher

# Install Docker (optional, for containerized services)
docker --version
```

### Running YARP Gateway

```bash
# Navigate to YARP gateway directory
cd yarp

# Restore dependencies
dotnet restore

# Run the gateway
dotnet run

# Or run in development mode with hot reload
dotnet watch run
```

**Gateway will be available at:**
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/docs

### Running Ocelot Gateway

```bash
# Navigate to Ocelot gateway directory
cd ocelot

# Restore dependencies
dotnet restore

# Run the gateway
dotnet run

# Or run in development mode
dotnet watch run
```

**Gateway will be available at:**
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/gateway-docs

## API Endpoints

### Service Routes

| Service | Route | Backend Port |
|---------|-------|--------------|
| HR Service | `/v1/hr/*` | 5001 |
| Inventory Service | `/v1/inventory/*` | 5002 |
| Accounting Service | `/v1/accounting/*` | 5003 |
| Workflow Service | `/v1/workflow/*` | 8080 |
| Notification Service | `/v1/notifications/*` | 3000 |

### Gateway Management

| Endpoint | Description |
|----------|-------------|
| `/health` | Gateway health check |
| `/gateway/info` | Gateway information |
| `/gateway/routes` | Available routes |
| `/health/services` | Backend service health |

### Example Requests

```bash
# Get all employees (requires authentication)
curl -H "Authorization: Bearer <jwt-token>" \
     http://localhost:5000/v1/hr/employees

# API Key authentication
curl -H "X-API-Key: dev-service-key-12345" \
     http://localhost:5000/v1/inventory/products

# Health check (no auth required)
curl http://localhost:5000/health

# Gateway information
curl http://localhost:5000/gateway/info
```

## Configuration

### Environment Variables

```bash
# Authentication settings
export JWT_AUTHORITY="https://login.microsoftonline.com/your-tenant-id"
export JWT_AUDIENCE="your-client-id"

# Backend service URLs (development)
export HR_SERVICE_URL="http://localhost:5001"
export INVENTORY_SERVICE_URL="http://localhost:5002"
export ACCOUNTING_SERVICE_URL="http://localhost:5003"
export WORKFLOW_SERVICE_URL="http://localhost:8080"
export NOTIFICATION_SERVICE_URL="http://localhost:3000"

# Rate limiting
export RATE_LIMIT_REQUESTS_PER_MINUTE="100"
export RATE_LIMIT_BURST="10"

# Logging
export LOG_LEVEL="Information"
export APPLICATION_INSIGHTS_CONNECTION_STRING="your-connection-string"
```

### YARP Configuration

The YARP gateway is configured via `appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "hr-service-route": {
        "ClusterId": "hr-cluster",
        "Match": { "Path": "/v1/hr/{**catch-all}" },
        "Transforms": [
          { "PathRemovePrefix": "/v1/hr" },
          { "PathPrefix": "/api/v1" }
        ]
      }
    },
    "Clusters": {
      "hr-cluster": {
        "Destinations": {
          "hr-destination": {
            "Address": "http://localhost:5001/"
          }
        }
      }
    }
  }
}
```

### Ocelot Configuration

The Ocelot gateway is configured via `ocelot.json`:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/{everything}",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5001 }
      ],
      "UpstreamPathTemplate": "/v1/hr/{everything}",
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ]
}
```

## Authentication

### JWT Bearer Token

```bash
# Get token from Azure AD (example)
TOKEN=$(curl -X POST \
  https://login.microsoftonline.com/your-tenant-id/oauth2/v2.0/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=your-client-id" \
  -d "client_secret=your-client-secret" \
  -d "scope=api://your-api-scope/.default" \
  -d "grant_type=client_credentials" | jq -r '.access_token')

# Use token in requests
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/v1/hr/employees
```

### API Key Authentication

```bash
# Use API key for service-to-service calls
curl -H "X-API-Key: dev-service-key-12345" \
     http://localhost:5000/v1/hr/employees
```

## Rate Limiting

### Default Limits

- **Default**: 100 requests per minute
- **HR Service**: 50 requests per minute (sensitive data)
- **Notification Service**: 200 requests per minute (high-frequency)

### Custom Rate Limiting

Rate limits can be configured per route in the configuration files:

```json
{
  "RateLimitOptions": {
    "PermitLimit": 100,
    "Window": "00:01:00",
    "QueueLimit": 10
  }
}
```

## Health Checks

### Gateway Health

```bash
# Check gateway health
curl http://localhost:5000/health

# Response
{
  "status": "Healthy",
  "totalDuration": "00:00:00.001",
  "entries": {
    "gateway-health": {
      "status": "Healthy",
      "description": "Gateway is running"
    }
  }
}
```

### Service Health Aggregation

```bash
# Check all services health
curl http://localhost:5000/health/services

# Response
{
  "status": "Healthy",
  "services": [
    {
      "service": "HR Service",
      "status": "Healthy",
      "statusCode": 200
    },
    {
      "service": "Inventory Service", 
      "status": "Healthy",
      "statusCode": 200
    }
  ]
}
```

## Load Balancing

### Multiple Backend Instances

Configure multiple destinations for load balancing:

```json
{
  "Clusters": {
    "hr-cluster": {
      "Destinations": {
        "hr-destination-1": { "Address": "http://localhost:5001/" },
        "hr-destination-2": { "Address": "http://localhost:5011/" },
        "hr-destination-3": { "Address": "http://localhost:5021/" }
      },
      "LoadBalancingPolicy": "RoundRobin"
    }
  }
}
```

## CORS Configuration

### Allowed Origins

The gateway is configured to allow requests from:
- `http://localhost:3000` (React dev server)
- `http://localhost:3001` (React admin portal)
- `http://localhost:4200` (Angular dev server)
- `https://erp-portal.com` (Production web app)
- `https://admin.erp-portal.com` (Production admin portal)

### Custom CORS Policy

```json
{
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Authorization", "Content-Type"],
    "AllowCredentials": true
  }
}
```

## Environment-Specific Routing

### Development Environment

```json
{
  "EnvironmentRouting": {
    "Development": {
      "HRService": "http://localhost:5001",
      "InventoryService": "http://localhost:5002"
    }
  }
}
```

### Kubernetes Environment

```json
{
  "EnvironmentRouting": {
    "Production": {
      "HRService": "http://hr-service.default.svc.cluster.local",
      "InventoryService": "http://inventory-service.default.svc.cluster.local"
    }
  }
}
```

## Monitoring and Logging

### Application Insights Integration

```json
{
  "Telemetry": {
    "ApplicationInsights": {
      "ConnectionString": "InstrumentationKey=your-key"
    }
  }
}
```

### Structured Logging

```bash
# View logs with correlation IDs
docker logs gateway-container --follow | grep "CorrelationId"

# Sample log output
[INFO] Request: GET /v1/hr/employees - Correlation ID: abc123
[INFO] Response: 200 - Duration: 45ms - Correlation ID: abc123
```

## Deployment

### Docker Deployment

```bash
# Build Docker image for YARP gateway
cd yarp
docker build -t erp-gateway-yarp:latest .

# Run with Docker
docker run -p 5000:5000 -p 5001:5001 \
  -e JWT_AUTHORITY="https://login.microsoftonline.com/your-tenant" \
  -e JWT_AUDIENCE="your-client-id" \
  erp-gateway-yarp:latest
```

### Kubernetes Deployment

```yaml
# gateway-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-gateway
spec:
  replicas: 3
  selector:
    matchLabels:
      app: api-gateway
  template:
    metadata:
      labels:
        app: api-gateway
    spec:
      containers:
      - name: gateway
        image: erp-gateway-yarp:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   ```bash
   # Check JWT token validity
   jwt-cli decode $TOKEN
   
   # Verify token claims
   curl -H "Authorization: Bearer $TOKEN" \
        http://localhost:5000/gateway/info
   ```

2. **Service Unavailable**
   ```bash
   # Check backend service health
   curl http://localhost:5001/health
   
   # Check gateway routing
   curl http://localhost:5000/gateway/routes
   ```

3. **Rate Limiting**
   ```bash
   # Check rate limit headers
   curl -I http://localhost:5000/v1/hr/employees
   
   # X-RateLimit-Limit: 100
   # X-RateLimit-Remaining: 99
   ```

### Debug Mode

```bash
# Run with debug logging
export ASPNETCORE_ENVIRONMENT=Development
export Logging__LogLevel__Default=Debug
dotnet run
```

## Performance Tuning

### Connection Pooling

```json
{
  "HttpClientOptions": {
    "MaxConnectionsPerServer": 10,
    "PooledConnectionLifetime": "00:05:00"
  }
}
```

### Response Compression

```json
{
  "CompressionOptions": {
    "EnableCompression": true,
    "CompressionLevel": "Optimal"
  }
}
```

This self-hosted gateway provides a complete API management solution for local development and testing of the ERP system microservices.
