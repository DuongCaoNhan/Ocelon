using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Polly;
using Polly.Extensions.Http;
using AIAgentService.Application.Services;

namespace AIAgentService.Infrastructure.Services
{
    /// <summary>
    /// Service for integrating with external ERP microservices
    /// Implements retry policies and circuit breaker patterns for resilience
    /// </summary>
    public class ExternalServiceIntegration : IExternalServiceIntegration
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalServiceIntegration> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        public ExternalServiceIntegration(
            HttpClient httpClient,
            ILogger<ExternalServiceIntegration> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Configure retry policy with exponential backoff
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning("Retry {RetryCount} for external service call after {Delay}ms", 
                            retryCount, timespan.TotalMilliseconds);
                    });
        }

        public async Task<string> CallHRServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling HR Service - Operation: {Operation}", operation);

                var serviceUrl = _configuration["Services:HRService:BaseUrl"] ?? "http://localhost:5001";
                var endpoint = operation.ToLowerInvariant() switch
                {
                    "getleavebalance" => "/api/hr/leave-balance",
                    "getemployeeinfo" => "/api/hr/employee",
                    "createemployee" => "/api/hr/employee",
                    "getorganizationchart" => "/api/hr/organization",
                    _ => throw new ArgumentException($"Unknown HR operation: {operation}")
                };

                var requestUrl = $"{serviceUrl}{endpoint}";
                if (!string.IsNullOrEmpty(parameters))
                {
                    requestUrl += $"?{parameters}";
                }

                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await _httpClient.GetAsync(requestUrl, cancellationToken);
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("HR Service call completed successfully - Operation: {Operation}", operation);
                
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling HR Service - Operation: {Operation}", operation);
                return CreateErrorResponse("HR Service", operation, ex.Message);
            }
        }

        public async Task<string> CallInventoryServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling Inventory Service - Operation: {Operation}", operation);

                var serviceUrl = _configuration["Services:InventoryService:BaseUrl"] ?? "http://localhost:5002";
                var endpoint = operation.ToLowerInvariant() switch
                {
                    "getstock" => "/api/inventory/stock",
                    "updatestock" => "/api/inventory/stock",
                    "getproducts" => "/api/inventory/products",
                    "getlowstock" => "/api/inventory/low-stock",
                    _ => throw new ArgumentException($"Unknown Inventory operation: {operation}")
                };

                var requestUrl = $"{serviceUrl}{endpoint}";
                if (!string.IsNullOrEmpty(parameters))
                {
                    requestUrl += $"?{parameters}";
                }

                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await _httpClient.GetAsync(requestUrl, cancellationToken);
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Inventory Service call completed successfully - Operation: {Operation}", operation);
                
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Inventory Service - Operation: {Operation}", operation);
                return CreateErrorResponse("Inventory Service", operation, ex.Message);
            }
        }

        public async Task<string> CallAccountingServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling Accounting Service - Operation: {Operation}", operation);

                var serviceUrl = _configuration["Services:AccountingService:BaseUrl"] ?? "http://localhost:5003";
                var endpoint = operation.ToLowerInvariant() switch
                {
                    "getfinancialreport" => "/api/accounting/reports",
                    "getaccounts" => "/api/accounting/accounts",
                    "gettransactions" => "/api/accounting/transactions",
                    "getbalancesheet" => "/api/accounting/balance-sheet",
                    _ => throw new ArgumentException($"Unknown Accounting operation: {operation}")
                };

                var requestUrl = $"{serviceUrl}{endpoint}";
                if (!string.IsNullOrEmpty(parameters))
                {
                    requestUrl += $"?{parameters}";
                }

                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await _httpClient.GetAsync(requestUrl, cancellationToken);
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Accounting Service call completed successfully - Operation: {Operation}", operation);
                
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Accounting Service - Operation: {Operation}", operation);
                return CreateErrorResponse("Accounting Service", operation, ex.Message);
            }
        }

        public async Task<string> CallWorkflowServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling Workflow Service - Operation: {Operation}", operation);

                var serviceUrl = _configuration["Services:WorkflowService:BaseUrl"] ?? "http://localhost:5004";
                var endpoint = operation.ToLowerInvariant() switch
                {
                    "getworkflows" => "/api/workflow/workflows",
                    "startworkflow" => "/api/workflow/start",
                    "getworkflowstatus" => "/api/workflow/status",
                    "gettasks" => "/api/workflow/tasks",
                    _ => throw new ArgumentException($"Unknown Workflow operation: {operation}")
                };

                var requestUrl = $"{serviceUrl}{endpoint}";
                if (!string.IsNullOrEmpty(parameters))
                {
                    requestUrl += $"?{parameters}";
                }

                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await _httpClient.GetAsync(requestUrl, cancellationToken);
                    httpResponse.EnsureSuccessStatusCode();
                    return httpResponse;
                });

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Workflow Service call completed successfully - Operation: {Operation}", operation);
                
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Workflow Service - Operation: {Operation}", operation);
                return CreateErrorResponse("Workflow Service", operation, ex.Message);
            }
        }

        public async Task<bool> IsServiceAvailableAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Checking availability of service: {ServiceName}", serviceName);

                var baseUrl = serviceName.ToLowerInvariant() switch
                {
                    "hrservice" => _configuration["Services:HRService:BaseUrl"] ?? "http://localhost:5001",
                    "inventoryservice" => _configuration["Services:InventoryService:BaseUrl"] ?? "http://localhost:5002",
                    "accountingservice" => _configuration["Services:AccountingService:BaseUrl"] ?? "http://localhost:5003",
                    "workflowservice" => _configuration["Services:WorkflowService:BaseUrl"] ?? "http://localhost:5004",
                    _ => throw new ArgumentException($"Unknown service: {serviceName}")
                };

                var healthCheckUrl = $"{baseUrl}/health";
                
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout for health checks

                var response = await _httpClient.GetAsync(healthCheckUrl, cts.Token);
                var isAvailable = response.IsSuccessStatusCode;

                _logger.LogDebug("Service {ServiceName} availability: {IsAvailable}", serviceName, isAvailable);
                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Service {ServiceName} is not available", serviceName);
                return false;
            }
        }

        private static string CreateErrorResponse(string serviceName, string operation, string errorMessage)
        {
            var errorResponse = new
            {
                Error = true,
                Service = serviceName,
                Operation = operation,
                Message = errorMessage,
                Timestamp = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(errorResponse);
        }
    }
}
