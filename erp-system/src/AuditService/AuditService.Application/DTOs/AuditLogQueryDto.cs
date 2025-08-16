using AuditService.Domain.Enums;

namespace AuditService.Application.DTOs;

/// <summary>
/// Data Transfer Object for querying audit logs
/// Provides filtering, pagination, and sorting capabilities
/// </summary>
public class AuditLogQueryDto
{
    /// <summary>
    /// Filter by user ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Filter by service name
    /// </summary>
    public string? ServiceName { get; set; }
    
    /// <summary>
    /// Filter by entity type
    /// </summary>
    public string? EntityType { get; set; }
    
    /// <summary>
    /// Filter by entity ID
    /// </summary>
    public string? EntityId { get; set; }
    
    /// <summary>
    /// Filter by action type
    /// </summary>
    public AuditActionType? ActionType { get; set; }
    
    /// <summary>
    /// Filter by result status
    /// </summary>
    public AuditResult? Result { get; set; }
    
    /// <summary>
    /// Filter by severity level
    /// </summary>
    public AuditSeverity? Severity { get; set; }
    
    /// <summary>
    /// Filter by correlation ID
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Start date for time range filter
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// End date for time range filter
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// Filter by IP address
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Filter by source system
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Filter by tags (comma-separated)
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Include entries with sensitive data
    /// </summary>
    public bool? IncludeSensitiveData { get; set; }
    
    /// <summary>
    /// Filter by retention category
    /// </summary>
    public string? RetentionCategory { get; set; }
    
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Number of records per page
    /// </summary>
    public int PageSize { get; set; } = 50;
    
    /// <summary>
    /// Sort field name
    /// </summary>
    public string SortBy { get; set; } = "Timestamp";
    
    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
