# Deploy API Gateway Infrastructure

## Prerequisites

- Azure CLI installed and logged in
- Appropriate permissions to create resources in the target subscription
- Resource group already created

## Deployment Steps

### 1. Create Resource Group (if not exists)

```bash
az group create --name ocelon-erp-rg --location "East US"
```

### 2. Deploy Infrastructure

```bash
# Deploy to development environment
az deployment group create \
  --resource-group ocelon-erp-rg \
  --template-file main.bicep \
  --parameters environmentName=dev

# Deploy to staging environment
az deployment group create \
  --resource-group ocelon-erp-rg \
  --template-file main.bicep \
  --parameters environmentName=staging

# Deploy to production environment
az deployment group create \
  --resource-group ocelon-erp-rg \
  --template-file main.bicep \
  --parameters environmentName=prod
```

### 3. Post-Deployment Configuration

#### Configure JWT Signing Key

```bash
# Set JWT signing key in Key Vault
az keyvault secret set \
  --vault-name ocelon-erp-apim-dev-kv \
  --name jwt-signing-key \
  --value "your-jwt-signing-key-here"
```

#### Update Backend Service URLs

```bash
# Update named values with actual backend URLs
az apim nv update \
  --service-name ocelon-erp-apim-dev \
  --resource-group ocelon-erp-rg \
  --named-value-id hr-service-backend-url \
  --value "https://your-hr-service-url.com"
```

### 4. Import API Policies

```bash
# Import global policy
az apim api policy import \
  --resource-group ocelon-erp-rg \
  --service-name ocelon-erp-apim-dev \
  --api-id hr-api \
  --policy-format xml \
  --policy-path ../api-management/policies/hr-service-policy.xml
```

## Verification

### Check APIM Status

```bash
az apim show \
  --resource-group ocelon-erp-rg \
  --name ocelon-erp-apim-dev \
  --query "provisioningState"
```

### Test API Endpoint

```bash
curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees
```

## Testing

### 1. Test API Gateway Health

```bash
# Test APIM health endpoint
curl -i https://ocelon-erp-apim-dev.azure-api.net/status-0123456789abcdef
```

### 2. Test API Authentication

```bash
# Test without subscription key (should return 401)
curl -i https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees

# Test with invalid subscription key (should return 401)
curl -H "Ocp-Apim-Subscription-Key: invalid-key" \
  https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees

# Test with valid subscription key (should return 200 or backend response)
curl -H "Ocp-Apim-Subscription-Key: YOUR_VALID_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees
```

### 3. Test Rate Limiting

```bash
# Send multiple requests rapidly to test throttling
for i in {1..20}; do
  curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
    https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees &
done
wait
```

### 4. Test All Service Endpoints

```bash
# HR Service
curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees

# Inventory Service
curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/inventory/api/products

# Accounting Service
curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/accounting/api/accounts

# Workflow Service
curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/workflow/api/processes

# Notification Service
curl -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/notifications/api/messages
```

### 5. Test JWT Authentication (if configured)

```bash
# Get JWT token from your authentication provider
JWT_TOKEN="your-jwt-token-here"

# Test API with JWT
curl -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY" \
  https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees
```

### 6. Test CORS (from browser console)

```javascript
// Test CORS from browser developer console
fetch('https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees', {
  method: 'GET',
  headers: {
    'Ocp-Apim-Subscription-Key': 'YOUR_SUBSCRIPTION_KEY'
  }
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

### 7. Load Testing with Artillery

Create `load-test.yml`:

```yaml
config:
  target: 'https://ocelon-erp-apim-dev.azure-api.net'
  phases:
    - duration: 60
      arrivalRate: 10
  defaults:
    headers:
      Ocp-Apim-Subscription-Key: 'YOUR_SUBSCRIPTION_KEY'

scenarios:
  - name: 'Test HR API'
    weight: 40
    flow:
      - get:
          url: '/hr/api/employees'
  - name: 'Test Inventory API'
    weight: 30
    flow:
      - get:
          url: '/inventory/api/products'
  - name: 'Test Notification API'
    weight: 30
    flow:
      - get:
          url: '/notifications/api/messages'
```

Run load test:

```bash
# Install Artillery (if not installed)
npm install -g artillery

# Run load test
artillery run load-test.yml
```

### 8. Test Monitoring and Alerting

```bash
# Generate some errors to test alerting
for i in {1..15}; do
  curl -H "Ocp-Apim-Subscription-Key: invalid-key" \
    https://ocelon-erp-apim-dev.azure-api.net/hr/api/employees
done

# Check Application Insights for metrics
az monitor app-insights query \
  --app ocelon-erp-apim-dev-appinsights \
  --analytics-query "requests | where timestamp > ago(1h) | summarize count() by resultCode"
```

### 9. Test Self-Hosted Gateways

```bash
# Test YARP gateway (if running locally)
curl http://localhost:5000/hr/api/employees

# Test Ocelot gateway (if running locally)
curl http://localhost:5001/hr/api/employees
```

### 10. Postman Collection Testing

Create a Postman collection with the following requests:

1. **Health Check**
   - GET: `{{baseUrl}}/status-0123456789abcdef`

2. **HR Service Tests**
   - GET: `{{baseUrl}}/hr/api/employees`
   - POST: `{{baseUrl}}/hr/api/employees`
   - PUT: `{{baseUrl}}/hr/api/employees/{id}`
   - DELETE: `{{baseUrl}}/hr/api/employees/{id}`

3. **Authentication Tests**
   - No auth (expect 401)
   - Invalid key (expect 401)
   - Valid key (expect 200)

4. **Rate Limiting Test**
   - Multiple rapid requests

Environment variables:
```json
{
  "baseUrl": "https://ocelon-erp-apim-dev.azure-api.net",
  "subscriptionKey": "your-subscription-key",
  "jwtToken": "your-jwt-token"
}
```

## Troubleshooting

### Common Issues

1. **502 Bad Gateway**
   - Check backend service availability
   - Verify backend URLs in named values

2. **401 Unauthorized**
   - Verify subscription key
   - Check JWT token validity
   - Review authentication policies

3. **429 Too Many Requests**
   - Rate limiting is working correctly
   - Adjust throttling policies if needed

4. **500 Internal Server Error**
   - Check Application Insights logs
   - Review policy configurations
   - Verify backend service health

### Diagnostic Commands

```bash
# Check APIM diagnostic logs
az apim api diagnostic list \
  --resource-group ocelon-erp-rg \
  --service-name ocelon-erp-apim-dev \
  --api-id hr-api

# View Application Insights logs
az monitor app-insights query \
  --app ocelon-erp-apim-dev-appinsights \
  --analytics-query "traces | where timestamp > ago(1h) | order by timestamp desc"
```

## Outputs

After successful deployment, you'll have:

- Azure API Management instance
- Application Insights for monitoring
- Key Vault for secrets management
- Configured products and APIs
- Monitoring alerts and dashboard
- All necessary policies applied

## Cleanup

```bash
# Remove all resources
az group delete --name ocelon-erp-rg --yes --no-wait
```
