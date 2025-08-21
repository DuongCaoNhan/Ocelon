# Ocelon ERP System Developer Instructions

**ALWAYS follow these instructions first and fallback to additional search and context gathering only if the information here is incomplete or found to be in error.**

## System Overview

Ocelon is a comprehensive microservices-based Enterprise Resource Planning (ERP) system using a polyglot architecture:

- **.NET 8 Services**: HR, Inventory, Accounting, Audit, AI Agent services
- **Java 21 Service**: Workflow service (Spring Boot 3.2)
- **Node.js Service**: Notification service (Express + Socket.IO)
- **API Gateway**: Azure APIM with self-hosted YARP and Ocelot gateways

## Working Effectively

### Prerequisites Validation
ALWAYS verify these prerequisites before building:
```bash
# Check .NET 8 SDK
dotnet --version  # Should be 8.0.x

# Check Node.js 18+
node --version    # Should be 20.19.x or higher
npm --version     # Should be 10.8.x or higher

# Check Java (WARNING: pom.xml requires Java 21 but system has Java 17)
java --version    # Currently 17.0.16

# Check Maven
mvn --version     # Should be 3.9.x

# Check Azure CLI
az --version      # Should be 2.76.x
```

### Bootstrap and Build the Repository

**1. .NET Solution Build (TESTED - WORKS)**
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system

# Restore dependencies (takes ~51 seconds)
dotnet restore ERP.sln
# NEVER CANCEL: Set timeout to 120+ seconds

# Build solution (takes ~18 seconds)  
dotnet build ERP.sln
# NEVER CANCEL: Set timeout to 60+ seconds
# WARNING: Expect 8 warnings about header dictionary usage - these are not errors

# Run tests (takes ~9 seconds, some tests currently fail)
dotnet test ERP.sln
# NEVER CANCEL: Set timeout to 30+ seconds
# NOTE: AIAgentService tests currently fail (2 failed), HRService tests pass (6 passed)
```

**2. Node.js Service Setup (TESTED - LIMITED)**
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system/src/NotificationService

# Install dependencies (takes ~31 seconds)
npm install
# NEVER CANCEL: Set timeout to 120+ seconds
# WARNING: Expect security vulnerabilities warnings

# Test command (currently no tests exist)
npm test
# NOTE: No tests found - exits with code 1

# Build command (no-op for Node.js)
npm run build
```

**3. Java Service Build (TESTED - FAILS)**
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system/src/WorkflowService

# Build with Maven (takes ~49 seconds, currently fails)
mvn clean install
# NEVER CANCEL: Set timeout to 180+ seconds
# FAILS: "Unable to find main class" - service is incomplete
# NOTE: pom.xml requires Java 21 but system has Java 17
```

### Running Services

**1. YARP Gateway (TESTED - WORKS)**
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system/gateway/self-hosted-gateway/yarp
dotnet run --urls="http://localhost:5000"
# Starts successfully on ports 5000 (HTTP) and 5001 (HTTPS)
# Health checks show warnings for missing backend services
```

**2. HR Service (TESTED - FAILS)**
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system/src/HRService/HRService.API
dotnet run
# FAILS: Cannot connect to SQL Server LocalDB (not available on Linux)
# Service requires database configuration for development
```

**3. Other Services**
- **NotificationService**: Source code missing (only package.json exists)
- **WorkflowService**: Main class missing, incomplete implementation
- **Inventory/Accounting/Audit Services**: Build but not tested for runtime

## Validation Scenarios

**ALWAYS perform these validation steps after making changes:**

### 1. .NET Solution Validation
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system
dotnet build ERP.sln && echo "✅ Build successful"
dotnet test ERP.sln --verbosity quiet && echo "✅ Tests passed" || echo "⚠️ Some tests failed - check output"
```

### 2. Gateway Validation  
```bash
cd /home/runner/work/Ocelon/Ocelon/erp-system/gateway/self-hosted-gateway/yarp
timeout 10s dotnet run --urls="http://localhost:5000" &
sleep 5
curl -f http://localhost:5000/ >/dev/null 2>&1 && echo "✅ YARP Gateway responsive" || echo "❌ Gateway not responding"
pkill -f "dotnet run"
```

### 3. Code Quality Validation
```bash
# Run from repository root
cd /home/runner/work/Ocelon/Ocelon/erp-system

# .NET linting (built into build process)
dotnet build --verbosity quiet --configuration Release

# Node.js linting
cd src/NotificationService && npm run lint 2>/dev/null || echo "⚠️ No lint script or files"
```

## Build System Details

### Solution Structure
The main solution file `ERP.sln` includes:
- **18 projects** across multiple services
- **Unit test projects** for HR and AIAgent services
- **Gateway projects** (YARP, Ocelot, Shared libraries)
- **Clean architecture** layers for each service (Domain, Application, Infrastructure, API)

### Expected Build Times
- **dotnet restore**: ~51 seconds (dependencies download)
- **dotnet build**: ~18 seconds (compilation)
- **dotnet test**: ~9 seconds (test execution)
- **npm install**: ~31 seconds (Node.js dependencies)
- **mvn clean install**: ~49 seconds (fails due to missing main class)

## Common Issues and Solutions

### 1. .NET Build Warnings
**Issue**: 8 warnings about IHeaderDictionary usage in gateway projects
**Solution**: These are warnings, not errors. Build succeeds. Use `Append()` instead of `Add()` for headers.

### 2. Test Failures
**Issue**: AIAgentService tests fail (2 out of 18)
**Solution**: These are pre-existing issues with domain validation. Tests expect exceptions that aren't thrown.

### 3. Database Connection Errors
**Issue**: HR Service fails with SQL Server connection errors
**Solution**: Requires actual SQL Server instance. Use `appsettings.Development.json` to configure alternative connection string.

### 4. Missing Source Code
**Issue**: NotificationService has package.json but no source files
**Solution**: Service implementation is incomplete. Only configuration exists.

### 5. Java Version Mismatch
**Issue**: pom.xml specifies Java 21, system has Java 17
**Solution**: Either update pom.xml to Java 17 or install Java 21. Current build fails.

## Service Configuration

### Service Ports and Endpoints
| Service | Port | Technology | Status |
|---------|------|------------|--------|
| HRService | 5001 | .NET 8 | ⚠️ Builds, DB connection fails |
| InventoryService | 5002 | .NET 8 | ⚠️ Not tested |
| AccountingService | 5003 | .NET 8 | ⚠️ Not tested |
| AuditService | 5004 | .NET 8 | ⚠️ Not tested |
| AIAgentService | 5005 | .NET 8 | ⚠️ Builds, tests partially fail |
| CopilotAgentService | 5006 | .NET 8 | ⚠️ Not implemented |
| WorkflowService | 8080 | Java 21 | ❌ Build fails |
| NotificationService | 3000 | Node.js | ❌ No source code |
| YARP Gateway | 5000/5001 | .NET 8 | ✅ Working |
| Ocelot Gateway | Various | .NET 8 | ⚠️ Not tested |

### Health Check Endpoints
All services should implement `/health` endpoints. Gateway expects these for health monitoring.

## Azure Infrastructure

### Available Tools
- **Azure CLI**: Version 2.76.0 installed and ready
- **Bicep Templates**: Located in `erp-system/infrastructure/bicep/`
- **Gateway Scripts**: Documentation exists in `erp-system/gateway/build/`

### Infrastructure Commands
**WARNING**: No actual build scripts found in gateway/build directory, only documentation.

See `erp-system/gateway/build/QUICKSTART.md` and `README.md` for deployment guidance.

## Key Files and Locations

### Critical Configuration Files
- **Main Solution**: `erp-system/ERP.sln`
- **Service Configuration**: `erp-system/docs/DOTNET-ENVIRONMENT-CONFIG.md`
- **Gateway Documentation**: `erp-system/gateway/build/README.md`
- **Node.js Config**: `erp-system/src/NotificationService/package.json`
- **Java Config**: `erp-system/src/WorkflowService/pom.xml`

### Architecture Documentation
- **System Overview**: `README.md` (root level)
- **Service Specs**: `erp-system/docs/api-specs/`
- **Architecture**: `erp-system/docs/architecture.md`

## Development Workflow

### Making Changes
1. **Always build first**: `dotnet build ERP.sln` to ensure starting point is stable
2. **Make minimal changes**: Focus on single service or component
3. **Test early**: Run `dotnet test` frequently during development
4. **Validate gateway**: Start YARP gateway to test service integration
5. **Check health**: Ensure services can start (even if they fail on missing dependencies)

### Code Navigation
- **Service Structure**: Each .NET service follows DDD with Domain/Application/Infrastructure/API layers
- **Shared Libraries**: `gateway/src/Gateway.Shared/` contains common utilities
- **Tests**: Located in `tests/unit/<ServiceName>/`
- **Configuration**: Service-specific appsettings files in each API project

### Before Committing
1. Run `dotnet build ERP.sln` - must succeed
2. Run `dotnet test ERP.sln` - check for new test failures
3. For Node.js changes: run `npm install && npm run lint` in NotificationService
4. Verify services can still start (even if they fail on missing dependencies)

## Troubleshooting

### Quick Diagnostic Commands
```bash
# Check if solution builds
cd /home/runner/work/Ocelon/Ocelon/erp-system && dotnet build ERP.sln --verbosity minimal

# Check test status
dotnet test ERP.sln --verbosity minimal --logger "console;verbosity=minimal"

# Check specific service
cd src/HRService/HRService.API && dotnet run --urls="http://localhost:5001" &
sleep 3 && curl http://localhost:5001/health; pkill -f "dotnet run"

# Check Node.js service structure
ls -la src/NotificationService/
```

### Environment Issues
- **SQL Server**: Linux environment cannot run LocalDB - expect database connection failures
- **Java Version**: System has Java 17, pom.xml expects Java 21
- **Missing Files**: Several services have configuration but no implementation
- **Port Conflicts**: Ensure only one instance of each service runs

**CRITICAL**: This is a development environment. Many services are incomplete and require external dependencies. Focus on build validation rather than full runtime testing.