namespace AuditService.Domain.Contracts;

/// <summary>
/// Repository interface for audit log operations
/// Provides async operations for querying and storing audit logs
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Stores an audit log entry asynchronously
    /// </summary>
    /// <param name="auditLog">The audit log to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task AddAsync(Entities.AuditLog auditLog, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stores multiple audit log entries asynchronously
    /// </summary>
    /// <param name="auditLogs">The audit logs to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task AddRangeAsync(IEnumerable<Entities.AuditLog> auditLogs, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves audit logs based on filter criteria
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="serviceName">Optional service name filter</param>
    /// <param name="entityType">Optional entity type filter</param>
    /// <param name="action">Optional action type filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged result of audit logs</returns>
    Task<(IEnumerable<Entities.AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(
        string? userId = null,
        string? serviceName = null,
        string? entityType = null,
        string? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves audit logs by correlation ID for request tracing
    /// </summary>
    /// <param name="correlationId">The correlation ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs for the correlation ID</returns>
    Task<IEnumerable<Entities.AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves audit logs for a specific entity
    /// </summary>
    /// <param name="entityId">The entity ID to search for</param>
    /// <param name="entityType">The entity type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs for the entity</returns>
    Task<IEnumerable<Entities.AuditLog>> GetByEntityAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
}
