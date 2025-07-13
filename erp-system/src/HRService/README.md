# HR Service

## Overview

The HR Service is a core component of the ERP Management System responsible for managing human resources operations including employee management, payroll processing, and leave management. Built using .NET 10 with Domain-Driven Design (DDD) architecture.

## Architecture

The service follows DDD principles with clean architecture:

- **Domain Layer** - Core business logic and entities
- **Application Layer** - Use cases, commands, queries, and DTOs
- **Infrastructure Layer** - Data access, external services, and cross-cutting concerns
- **API Layer** - REST API endpoints and HTTP concerns

## Technology Stack

- **.NET 10** - Runtime framework
- **Entity Framework Core** - ORM for data access
- **MediatR** - Command/Query pattern implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging
- **Application Insights** - Monitoring and telemetry
- **xUnit** - Testing framework

## Features

### Employee Management
- Create, update, and delete employees
- Employee profile management
- Department and position management
- Employee search and filtering

### Payroll Management
- Payroll record creation and management
- Salary calculations
- Tax withholding management

### Leave Management
- Leave request submission
- Leave approval workflow
- Leave balance tracking

## API Endpoints

### Employees
- `GET /api/v1/employees` - Get all employees
- `GET /api/v1/employees/{id}` - Get employee by ID
- `GET /api/v1/employees/by-email/{email}` - Get employee by email
- `GET /api/v1/employees/by-department/{department}` - Get employees by department
- `POST /api/v1/employees` - Create new employee
- `PUT /api/v1/employees/{id}` - Update employee
- `DELETE /api/v1/employees/{id}` - Delete employee

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Environment (Development, Staging, Production)
- `ConnectionStrings__DefaultConnection` - Database connection string
- `Jwt__Key` - JWT signing key
- `Jwt__Issuer` - JWT issuer
- `Jwt__Audience` - JWT audience
- `ApplicationInsights__ConnectionString` - Application Insights connection string
- `Azure__ServiceBus__ConnectionString` - Azure Service Bus connection string
- `Azure__KeyVault__VaultUrl` - Azure Key Vault URL

### Database Configuration

The service uses SQL Server as the primary database. Connection strings can be configured in:
- `appsettings.json` for base configuration
- `appsettings.{Environment}.json` for environment-specific settings
- Environment variables for secure deployment

## Local Development

### Prerequisites
- .NET 10 SDK
- SQL Server or SQL Server LocalDB
- Docker (optional)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd erp-system/src/HRService
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   Update the connection string in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HRServiceDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update --project HRService.Infrastructure --startup-project HRService.API
   ```

5. **Run the application**
   ```bash
   cd HRService.API
   dotnet run
   ```

6. **Access the API**
   - Swagger UI: `https://localhost:5001`
   - Health Check: `https://localhost:5001/health`

## Docker Deployment

### Build Docker Image
```bash
# From the root of the erp-system directory
docker build -f src/HRService/Dockerfile -t hrservice:latest .
```

### Run Docker Container
```bash
docker run -d \
  --name hrservice \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  hrservice:latest
```

## Testing

### Run Unit Tests
```bash
dotnet test tests/unit/HRService/
```

### Run Integration Tests
```bash
dotnet test tests/integration/HRService/
```

### Run All Tests
```bash
dotnet test
```

## Security

### Authentication
- JWT Bearer token authentication
- Token validation with configurable issuer and audience

### Authorization
- Role-based access control
- Department-level access restrictions

### Data Protection
- Sensitive data encryption at rest
- HTTPS enforcement
- Input validation and sanitization

## Monitoring and Logging

### Structured Logging
- Serilog for structured logging
- Application Insights integration
- Request/response logging

### Health Checks
- Database connectivity checks
- Custom health check endpoints
- Kubernetes readiness/liveness probes

### Metrics
- Application performance metrics
- Business metrics (employee count, payroll totals)
- Custom telemetry

## API Documentation

API documentation is available through:
- **Swagger UI** - Interactive API documentation at `/swagger`
- **OpenAPI Specification** - Machine-readable API spec at `/swagger/v1/swagger.json`

## Database Schema

### Employee Table
- Employee personal information
- Employment details
- Address and emergency contact (owned entities)

### PayrollRecord Table
- Payroll processing records
- Salary and deduction details
- Pay period information

### LeaveRequest Table
- Leave application records
- Approval workflow status
- Leave type and duration

## Error Handling

### Global Exception Handling
- Centralized error handling middleware
- Consistent error response format
- Error logging and tracking

### Validation
- FluentValidation for input validation
- Model state validation
- Business rule validation

## Performance Considerations

### Database
- Indexed columns for frequent queries
- Query optimization
- Connection pooling

### Caching
- Response caching for read-heavy operations
- Distributed caching for scalability

### Async Operations
- Async/await pattern throughout
- Non-blocking I/O operations

## Contributing

1. Follow the established DDD patterns
2. Maintain test coverage above 80%
3. Use conventional commit messages
4. Update documentation for API changes
5. Follow C# coding standards and conventions

## Troubleshooting

### Common Issues

**Database Connection Issues**
- Verify connection string configuration
- Check SQL Server availability
- Ensure database exists and migrations are applied

**Authentication Issues**
- Verify JWT configuration
- Check token expiration
- Validate issuer/audience settings

**Performance Issues**
- Check database query performance
- Monitor memory usage
- Review logging levels

### Support

For support and questions:
- Check the main [ERP System documentation](../../docs/)
- Create an issue in the repository
- Contact the development team
