# Notification Service

## Overview

The Notification Service provides real-time communication capabilities for the ERP system, including email notifications, SMS, push notifications, and WebSocket-based real-time updates. Built with Node.js and designed for high throughput and real-time performance.

## Technology Stack

- **Node.js 18+** - JavaScript runtime
- **Express.js** - Web application framework
- **Socket.IO** - Real-time bidirectional communication
- **MongoDB** - Document database for notification logs
- **Nodemailer** - Email sending capabilities
- **Twilio** - SMS and communication APIs
- **Azure Service Bus** - Message queue integration
- **Winston** - Logging framework
- **Jest** - Testing framework

## Features

### Real-time Notifications
- WebSocket connections for instant updates
- User-specific notification channels
- Broadcast messaging capabilities
- Connection management and scaling

### Email Notifications
- HTML and text email templates
- Bulk email sending
- Email delivery tracking
- Attachment support

### SMS Notifications
- SMS delivery via Twilio
- International SMS support
- Delivery confirmation
- Cost optimization

### Push Notifications
- Web push notifications
- Mobile app integration
- Rich notification content
- Notification scheduling

## API Endpoints

### Notifications
- `POST /api/v1/notifications` - Send notification
- `GET /api/v1/notifications/{userId}` - Get user notifications
- `PUT /api/v1/notifications/{id}/read` - Mark notification as read
- `DELETE /api/v1/notifications/{id}` - Delete notification

### Email
- `POST /api/v1/email/send` - Send email
- `POST /api/v1/email/bulk` - Send bulk emails
- `GET /api/v1/email/templates` - Get email templates
- `POST /api/v1/email/templates` - Create email template

### SMS
- `POST /api/v1/sms/send` - Send SMS
- `GET /api/v1/sms/status/{id}` - Get SMS delivery status

### WebSocket Events
- `notification` - Real-time notification delivery
- `user_status` - User online/offline status
- `system_alert` - System-wide alerts

## Configuration

### Environment Variables

- `NODE_ENV` - Environment (development, staging, production)
- `PORT` - Server port (default: 3000)
- `MONGODB_URI` - MongoDB connection string
- `REDIS_URL` - Redis connection for session storage
- `JWT_SECRET` - JWT token secret
- `AZURE_SERVICEBUS_CONNECTION_STRING` - Azure Service Bus connection
- `AZURE_KEYVAULT_URL` - Azure Key Vault URL
- `AZURE_APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection

### Email Configuration
- `SMTP_HOST` - SMTP server host
- `SMTP_PORT` - SMTP server port
- `SMTP_USER` - SMTP username
- `SMTP_PASS` - SMTP password
- `EMAIL_FROM` - Default sender email

### SMS Configuration
- `TWILIO_ACCOUNT_SID` - Twilio Account SID
- `TWILIO_AUTH_TOKEN` - Twilio Auth Token
- `TWILIO_PHONE_NUMBER` - Twilio phone number

## Local Development

### Prerequisites
- Node.js 18+ and npm
- MongoDB (local or cloud)
- Redis (optional, for development)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd erp-system/src/NotificationService
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure environment**
   Create `.env` file:
   ```env
   NODE_ENV=development
   PORT=3000
   MONGODB_URI=mongodb://localhost:27017/notification_service_dev
   JWT_SECRET=your-jwt-secret
   SMTP_HOST=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USER=your-email@gmail.com
   SMTP_PASS=your-app-password
   EMAIL_FROM=noreply@erp-system.com
   ```

4. **Start development server**
   ```bash
   npm run dev
   ```

5. **Access the service**
   - API: `http://localhost:3000/api/v1`
   - Health Check: `http://localhost:3000/health`
   - WebSocket: `ws://localhost:3000`

## Docker Deployment

### Build Docker Image
```bash
docker build -t notification-service:latest .
```

### Run Docker Container
```bash
docker run -d \
  --name notification-service \
  -p 3000:3000 \
  -e NODE_ENV=production \
  -e MONGODB_URI="mongodb://your-mongo-server:27017/notification_service" \
  -e JWT_SECRET="your-jwt-secret" \
  notification-service:latest
```

## Message Queue Integration

### Azure Service Bus
- Subscribes to ERP system events
- Processes notification requests
- Handles message retries and dead letter queues
- Scales based on message volume

### Event Types
- `user.created` - New user registration
- `employee.hired` - Employee onboarding
- `invoice.created` - New invoice notifications
- `payment.received` - Payment confirmations
- `system.maintenance` - System maintenance alerts

## Notification Templates

### Email Templates
- Welcome email for new users
- Password reset notifications
- Invoice and payment confirmations
- System maintenance notifications
- Weekly/monthly reports

### SMS Templates
- OTP verification codes
- Urgent system alerts
- Payment due reminders
- Appointment confirmations

## Real-time Features

### WebSocket Connections
- User authentication via JWT
- Room-based messaging
- Connection pooling
- Automatic reconnection

### Presence Management
- User online/offline status
- Last seen timestamps
- Active session tracking

## Testing

### Unit Tests
```bash
npm test
```

### Integration Tests
```bash
npm run test:integration
```

### Coverage Report
```bash
npm run test:coverage
```

### Load Testing
```bash
npm run test:load
```

## Performance Considerations

### Scalability
- Horizontal scaling with load balancer
- Redis for session sharing
- MongoDB sharding for large datasets
- Connection pooling and management

### Optimization
- Message batching for bulk operations
- Caching for templates and user preferences
- Async processing for non-critical notifications
- Rate limiting for API endpoints

## Monitoring and Observability

### Metrics
- Notification delivery rates
- WebSocket connection counts
- Email/SMS success rates
- API response times

### Logging
- Structured logging with Winston
- Application Insights integration
- Error tracking and alerting
- Performance monitoring

### Health Checks
- Database connectivity
- External service availability
- Message queue health
- Memory and CPU usage

## Security

### Authentication
- JWT token validation
- API key authentication for external services
- Rate limiting per user/IP

### Data Protection
- PII encryption at rest
- Secure credential storage
- GDPR compliance considerations
- Audit logging

## Error Handling

### Retry Logic
- Exponential backoff for failed deliveries
- Dead letter queue for undeliverable messages
- Manual retry capabilities
- Error notification to administrators

### Fallback Mechanisms
- Multiple email providers
- SMS provider failover
- Graceful degradation for real-time features

## API Documentation

### OpenAPI Specification
```yaml
openapi: 3.0.0
info:
  title: Notification Service API
  version: 1.0.0
  description: Real-time notification and communication service
```

### Postman Collection
- Complete API collection available
- Environment configurations
- Sample requests and responses

## Troubleshooting

### Common Issues

**WebSocket Connection Issues**
- Check CORS configuration
- Verify JWT token validity
- Review firewall settings

**Email Delivery Issues**
- Verify SMTP configuration
- Check spam filters
- Review email template formatting

**SMS Delivery Issues**
- Verify Twilio credentials
- Check phone number format
- Review delivery reports

### Debug Mode
```bash
DEBUG=notification:* npm run dev
```

## Future Enhancements

- Rich media notifications
- Notification scheduling and queuing
- Advanced template engine
- Multi-language support
- Analytics dashboard
- Integration with more communication providers

## Contributing

1. Follow Node.js best practices
2. Write comprehensive tests
3. Document API changes
4. Use conventional commit messages
5. Update environment configurations

## Support

For support and questions:
- Check the main [ERP System documentation](../../docs/)
- Review Node.js and Socket.IO documentation
- Create an issue in the repository
- Contact the development team
