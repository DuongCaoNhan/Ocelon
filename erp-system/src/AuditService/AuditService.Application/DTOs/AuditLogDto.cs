using AuditService.Domain.Enums;

namespace AuditService.Application.DTOs;

/// <summary>
/// Data Transfer Object for audit log responses
/// Provides a clean interface for API consumers
/// </summary>
public class AuditLogDto
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Correlation ID for request tracing
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Name of the service that generated the event
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// User display name
    /// </summary>
    public string? UserDisplayName { get; set; }
    
    /// <summary>
    /// Type of action performed
    /// </summary>
    public AuditActionType ActionType { get; set; }
    
    /// <summary>
    /// Result of the operation
    /// </summary>
    public AuditResult Result { get; set; }
    
    /// <summary>
    /// Severity level
    /// </summary>
    public AuditSeverity Severity { get; set; }
    
    /// <summary>
    /// Type of entity being audited
    /// </summary>
    public string? EntityType { get; set; }
    
    /// <summary>
    /// ID of the entity being audited
    /// </summary>
    public string? EntityId { get; set; }
    
    /// <summary>
    /// Name of the entity for readability
    /// </summary>
    public string? EntityName { get; set; }
    
    /// <summary>
    /// Description of the action performed
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Reason for the action
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// IP address of the client
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Source system
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }
    
    /// <summary>
    /// Tags for categorization
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Indicates if this entry contains sensitive data
    /// </summary>
    public bool ContainsSensitiveData { get; set; }
    
    /// <summary>
    /// Data retention category
    /// </summary>
    public string? RetentionCategory { get; set; }
}

/// <summary>
/// Paged result for audit log queries
/// </summary>
public class PagedAuditLogDto
{
    /// <summary>
    /// List of audit logs for the current page
    /// </summary>
    public IEnumerable<AuditLogDto> Logs { get; set; } = Enumerable.Empty<AuditLogDto>();
    
    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Number of records per page
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of records across all pages
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
