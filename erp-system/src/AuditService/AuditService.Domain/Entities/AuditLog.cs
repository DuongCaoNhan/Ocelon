using AuditService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuditService.Domain.Entities;

/// <summary>
/// Core audit log entity that captures all audit trail information
/// Designed for high-volume, compliance-ready audit logging
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Correlation ID for request tracing across services
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the audit event occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Name of the service that generated the audit event
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID from Azure AD or service principal
    /// </summary>
    [MaxLength(100)]
    public string? UserId { get; set; }
    
    /// <summary>
    /// Display name of the user (for readability)
    /// </summary>
    [MaxLength(200)]
    public string? UserDisplayName { get; set; }
    
    /// <summary>
    /// Azure AD tenant ID
    /// </summary>
    [MaxLength(100)]
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Type of action performed
    /// </summary>
    public AuditActionType ActionType { get; set; }
    
    /// <summary>
    /// Result of the operation
    /// </summary>
    public AuditResult Result { get; set; }
    
    /// <summary>
    /// Severity level of the audit event
    /// </summary>
    public AuditSeverity Severity { get; set; }
    
    /// <summary>
    /// Type of entity being audited
    /// </summary>
    [MaxLength(100)]
    public string? EntityType { get; set; }
    
    /// <summary>
    /// ID of the entity being audited
    /// </summary>
    [MaxLength(100)]
    public string? EntityId { get; set; }
    
    /// <summary>
    /// Name or title of the entity (for readability)
    /// </summary>
    [MaxLength(500)]
    public string? EntityName { get; set; }
    
    /// <summary>
    /// Description of the action performed
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Reason for the action (if provided)
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// IP address of the client
    /// </summary>
    [MaxLength(45)] // IPv6 max length
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string from the client
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Source system or application that initiated the action
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }
    
    /// <summary>
    /// JSON representation of the original data (before changes)
    /// </summary>
    public string? OriginalData { get; set; }
    
    /// <summary>
    /// JSON representation of the new data (after changes)
    /// </summary>
    public string? NewData { get; set; }
    
    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Stack trace if an exception occurred
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }
    
    /// <summary>
    /// Tags for categorization and filtering
    /// </summary>
    [MaxLength(1000)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Indicates if this entry contains sensitive data (for retention policies)
    /// </summary>
    public bool ContainsSensitiveData { get; set; }
    
    /// <summary>
    /// Data retention category (for compliance)
    /// </summary>
    [MaxLength(50)]
    public string? RetentionCategory { get; set; }
    
    /// <summary>
    /// Calculated expiry date based on retention policies
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// Hash of the audit data for integrity verification
    /// </summary>
    [MaxLength(128)]
    public string? DataHash { get; set; }
    
    /// <summary>
    /// Version of the audit schema
    /// </summary>
    [MaxLength(20)]
    public string SchemaVersion { get; set; } = "1.0";
}
