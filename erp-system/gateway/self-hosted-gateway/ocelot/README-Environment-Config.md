# Ocelot Gateway Environment Configuration

This Ocelot gateway supports environment-specific configurations to enable different behaviors across Development, Staging, and Production environments.

## Configuration Files

### Environment-Specific Ocelot Configurations

- `ocelot.Development.json` - Development environment configuration
- `ocelot.Staging.json` - Staging environment configuration  
- `ocelot.Production.json` - Production environment configuration
- `ocelot.json` - Base/fallback configuration (optional)
- `ocelot.local.json` - Local developer overrides (optional, not committed to git)

### Environment-Specific App Settings

- `appsettings.Development.json` - Development-specific settings
- `appsettings.Staging.json` - Staging-specific settings
- `appsettings.Production.json` - Production-specific settings

## Configuration Loading Order

1. `ocelot.json` (base configuration, if exists)
2. `ocelot.{Environment}.json` (environment-specific, required)
3. `ocelot.local.json` (local overrides, optional)

## Key Differences by Environment

### Development
- **Rate Limiting**: Disabled for easier testing
- **Caching**: Short TTL (60 seconds)
- **Error Handling**: Detailed error messages enabled
- **Logging**: Verbose logging (Debug level)
- **Security**: Relaxed CORS, no HTTPS requirement
- **Downstream Services**: localhost endpoints
- **Circuit Breaker**: More tolerant (10 exceptions before breaking)

### Staging
- **Rate Limiting**: Moderate limits enabled
- **Caching**: Medium TTL (300 seconds)
- **Error Handling**: Generic error messages
- **Logging**: Information level
- **Security**: HTTPS required, security headers enabled
- **Downstream Services**: staging domain endpoints with backup instances
- **Circuit Breaker**: Production-like settings (5 exceptions before breaking)

### Production
- **Rate Limiting**: Strict limits enforced
- **Caching**: Long TTL (600 seconds)
- **Error Handling**: Minimal error details
- **Logging**: Warning level only
- **Security**: Full security headers, strict CORS, HTTPS required
- **Downstream Services**: production domains with multiple backup instances and DR
- **Circuit Breaker**: Conservative settings (3 exceptions before breaking)

## Environment-Specific Features

### Rate Limiting
- **Development**: Disabled (`EnableRateLimiting: false`)
- **Staging**: Moderate limits (100-250 requests/minute)
- **Production**: Strict limits (50-200 requests/minute)

### Service Discovery
- **Development**: Configuration-based (no external service discovery)
- **Staging**: Consul integration (`consul-staging.ocelon.com`)
- **Production**: Consul integration (`consul.ocelon.com`)

### Load Balancing
- **Development**: Single instance per service
- **Staging**: Primary + backup instances
- **Production**: Primary + backup + disaster recovery instances

### Security Headers
- **Development**: Minimal security headers
- **Staging**: Standard security headers
- **Production**: Full security suite including CSP, HSTS, etc.

## Running with Specific Environment

### Set Environment Variable

```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_ENVIRONMENT=Staging
set ASPNETCORE_ENVIRONMENT=Production

# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_ENVIRONMENT=Staging
export ASPNETCORE_ENVIRONMENT=Production
```

### Using Launch Profiles

```json
{
  "profiles": {
    "Development": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "Staging": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Staging"
      }
    },
    "Production": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    }
  }
}
```

## Local Development Overrides

For local development, you can create a `ocelot.local.json` file to override specific settings without affecting the committed configuration files.

Example `ocelot.local.json`:
```json
{
  "Routes": [
    {
      "DownstreamHostAndPorts": [
        {
          "Host": "my-local-service",
          "Port": 8080
        }
      ],
      "Key": "hr-service"
    }
  ]
}
```

## Monitoring and Health Checks

Each environment has different health check configurations:

- **Development**: `/health/all` aggregates all service health checks
- **Staging**: Individual service health endpoints + aggregated view
- **Production**: Comprehensive health monitoring with alerting

## Swagger Documentation

- **Development**: Enabled with detailed API documentation
- **Staging**: Enabled for testing purposes
- **Production**: Disabled for security

## Best Practices

1. **Never commit `ocelot.local.json`** - Add it to `.gitignore`
2. **Use environment variables** for sensitive configuration values
3. **Test configuration changes** in Development before promoting to Staging
4. **Monitor rate limits** and adjust based on actual usage patterns
5. **Keep security headers updated** in Production configuration
6. **Regularly review and update** circuit breaker settings based on service reliability

## Troubleshooting

### Configuration Not Loading
1. Verify the environment variable is set correctly
2. Check that the environment-specific JSON file exists
3. Validate JSON syntax in configuration files
4. Check application logs for configuration loading errors

### Service Discovery Issues
1. Ensure Consul is running and accessible
2. Verify service registration in Consul
3. Check network connectivity to Consul endpoints
4. Review Consul configuration in GlobalConfiguration section

### Rate Limiting Problems
1. Check rate limit settings for your environment
2. Verify client identification headers
3. Review rate limit logs
4. Consider whitelist entries for testing

## Security Considerations

### Development
- Use local certificates for HTTPS testing
- Keep development keys separate from production
- Enable detailed logging for debugging

### Staging
- Use staging certificates
- Test security headers and CORS policies
- Validate authentication and authorization flows

### Production
- Use production certificates from trusted CA
- Enable all security headers
- Monitor for security violations
- Regular security audits of configuration
