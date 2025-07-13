# Workflow Service

## Overview

The Workflow Service provides business process automation and workflow management capabilities for the ERP system. Built with Java and Spring Boot, it manages complex business processes across multiple services.

## Technology Stack

- **Java 21** - Programming language
- **Spring Boot 3.2** - Application framework
- **Flowable** - Business Process Management (BPM) engine
- **Spring Data JPA** - Data access layer
- **Spring Security** - Authentication and authorization
- **SQL Server** - Database
- **Maven** - Build tool
- **Docker** - Containerization

## Features

### Process Management
- Business process definition and execution
- Process instance tracking and monitoring
- Task assignment and completion
- Process versioning and deployment

### Workflow Automation
- Automated task routing
- Conditional process flows
- Integration with external services
- Event-driven process triggers

### Task Management
- Human task assignment
- Task priority and deadline management
- Task delegation and escalation
- Task completion tracking

## API Endpoints

### Process Definitions
- `GET /api/v1/process-definitions` - Get all process definitions
- `POST /api/v1/process-definitions` - Deploy new process definition
- `GET /api/v1/process-definitions/{id}` - Get process definition details

### Process Instances
- `GET /api/v1/process-instances` - Get process instances
- `POST /api/v1/process-instances` - Start new process instance
- `GET /api/v1/process-instances/{id}` - Get process instance details
- `DELETE /api/v1/process-instances/{id}` - Cancel process instance

### Tasks
- `GET /api/v1/tasks` - Get user tasks
- `POST /api/v1/tasks/{id}/complete` - Complete task
- `POST /api/v1/tasks/{id}/claim` - Claim task
- `POST /api/v1/tasks/{id}/delegate` - Delegate task

### Process Variables
- `GET /api/v1/process-instances/{id}/variables` - Get process variables
- `PUT /api/v1/process-instances/{id}/variables` - Update process variables

## Configuration

### Environment Variables

- `SPRING_PROFILES_ACTIVE` - Active Spring profile (dev, staging, prod)
- `SPRING_DATASOURCE_URL` - Database connection URL
- `SPRING_DATASOURCE_USERNAME` - Database username
- `SPRING_DATASOURCE_PASSWORD` - Database password
- `AZURE_SERVICEBUS_CONNECTION_STRING` - Azure Service Bus connection
- `AZURE_KEYVAULT_URL` - Azure Key Vault URL
- `AZURE_APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection

### Application Properties

```properties
# Server Configuration
server.port=8082
server.servlet.context-path=/workflow-service

# Database Configuration
spring.datasource.url=jdbc:sqlserver://localhost:1433;databaseName=WorkflowServiceDb
spring.datasource.driver-class-name=com.microsoft.sqlserver.jdbc.SQLServerDriver
spring.jpa.hibernate.ddl-auto=update
spring.jpa.show-sql=false

# Flowable Configuration
flowable.database-schema-update=true
flowable.async-executor-activate=true
flowable.history-level=full

# Security Configuration
spring.security.oauth2.resourceserver.jwt.issuer-uri=https://your-auth-server.com

# Actuator Configuration
management.endpoints.web.exposure.include=health,info,metrics,prometheus
management.endpoint.health.show-details=always
```

## Local Development

### Prerequisites
- Java 21 JDK
- Maven 3.9+
- SQL Server or Docker
- IDE (IntelliJ IDEA, Eclipse, VS Code)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd erp-system/src/WorkflowService
   ```

2. **Install dependencies**
   ```bash
   mvn clean install
   ```

3. **Configure database**
   Update `application-dev.properties` with your database connection:
   ```properties
   spring.datasource.url=jdbc:sqlserver://localhost:1433;databaseName=WorkflowServiceDb_Dev
   spring.datasource.username=your-username
   spring.datasource.password=your-password
   ```

4. **Run the application**
   ```bash
   mvn spring-boot:run -Dspring-boot.run.profiles=dev
   ```

5. **Access the application**
   - API: `http://localhost:8082/workflow-service`
   - Swagger UI: `http://localhost:8082/workflow-service/swagger-ui.html`
   - Health Check: `http://localhost:8082/workflow-service/actuator/health`

## Docker Deployment

### Build Docker Image
```bash
# From the WorkflowService directory
mvn clean package
docker build -t workflow-service:latest .
```

### Run Docker Container
```bash
docker run -d \
  --name workflow-service \
  -p 8082:8082 \
  -e SPRING_PROFILES_ACTIVE=prod \
  -e SPRING_DATASOURCE_URL="jdbc:sqlserver://your-db-server:1433;databaseName=WorkflowServiceDb" \
  -e SPRING_DATASOURCE_USERNAME="your-username" \
  -e SPRING_DATASOURCE_PASSWORD="your-password" \
  workflow-service:latest
```

## Process Definitions

### BPMN 2.0 Support
- Standard BPMN 2.0 process definitions
- Visual process designer integration
- Process validation and deployment
- Version management

### Sample Process Types
- Employee onboarding workflow
- Purchase order approval process
- Leave request approval workflow
- Invoice processing automation

## Integration

### Service Communication
- REST API calls to other ERP services
- Message queue integration (Azure Service Bus)
- Event-driven architecture support
- Synchronous and asynchronous communication

### External Systems
- Email notifications
- Document management systems
- Third-party approval systems
- Legacy system integration

## Testing

### Unit Tests
```bash
mvn test
```

### Integration Tests
```bash
mvn test -Dtest="*IntegrationTest"
```

### Test Containers
- Automated integration testing with real databases
- Isolated test environments
- Cleanup after test execution

## Monitoring and Observability

### Metrics
- Process execution metrics
- Task completion rates
- Process performance statistics
- Custom business metrics

### Logging
- Structured logging with Logback
- Process audit trails
- Task activity logging
- Error tracking and alerting

### Health Checks
- Database connectivity
- Process engine health
- External service availability
- Custom health indicators

## Security

### Authentication
- JWT token validation
- Integration with identity provider
- Service-to-service authentication

### Authorization
- Role-based access control
- Process-level permissions
- Task assignment rules
- Data access restrictions

## Performance Considerations

### Optimization
- Process definition caching
- Database connection pooling
- Async task execution
- Batch processing capabilities

### Scalability
- Horizontal scaling support
- Load balancing considerations
- Database partitioning strategies
- Caching mechanisms

## Troubleshooting

### Common Issues

**Process Engine Issues**
- Check database connectivity
- Verify process definition syntax
- Review process variable data types

**Performance Issues**
- Monitor database queries
- Check process complexity
- Review task assignment logic

**Integration Issues**
- Verify external service connectivity
- Check message queue configuration
- Review authentication tokens

### Logging Levels
```properties
# Enable debug logging for troubleshooting
logging.level.org.flowable=DEBUG
logging.level.com.erpsystem.workflow=DEBUG
```

## Future Enhancements

- Process analytics and reporting
- Machine learning integration for process optimization
- Advanced workflow templates
- Mobile task management
- Real-time process monitoring dashboard

## Contributing

1. Follow Java coding standards
2. Write comprehensive tests
3. Document process definitions
4. Use conventional commit messages
5. Update API documentation for changes

## Support

For support and questions:
- Check the main [ERP System documentation](../../docs/)
- Review Flowable documentation
- Create an issue in the repository
- Contact the development team
