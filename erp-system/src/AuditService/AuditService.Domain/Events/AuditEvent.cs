using AuditService.Domain.Enums;
using System.Text.Json.Serialization;

namespace AuditService.Domain.Events;

/// <summary>
/// Audit event for publishing to message broker
/// Lightweight version of AuditLog for cross-service communication
/// </summary>
public class AuditEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Correlation ID for request tracing
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Source service name
    /// </summary>
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// User identifier
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
    
    /// <summary>
    /// User display name
    /// </summary>
    [JsonPropertyName("userDisplayName")]
    public string? UserDisplayName { get; set; }
    
    /// <summary>
    /// Azure AD tenant ID
    /// </summary>
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Type of action performed
    /// </summary>
    [JsonPropertyName("actionType")]
    public AuditActionType ActionType { get; set; }
    
    /// <summary>
    /// Result of the operation
    /// </summary>
    [JsonPropertyName("result")]
    public AuditResult Result { get; set; }
    
    /// <summary>
    /// Severity level
    /// </summary>
    [JsonPropertyName("severity")]
    public AuditSeverity Severity { get; set; }
    
    /// <summary>
    /// Entity type being audited
    /// </summary>
    [JsonPropertyName("entityType")]
    public string? EntityType { get; set; }
    
    /// <summary>
    /// Entity ID being audited
    /// </summary>
    [JsonPropertyName("entityId")]
    public string? EntityId { get; set; }
    
    /// <summary>
    /// Entity name for readability
    /// </summary>
    [JsonPropertyName("entityName")]
    public string? EntityName { get; set; }
    
    /// <summary>
    /// Description of the action
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Reason for the action
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Client IP address
    /// </summary>
    [JsonPropertyName("ipAddress")]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string
    /// </summary>
    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Source system
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    /// <summary>
    /// Original data (JSON)
    /// </summary>
    [JsonPropertyName("originalData")]
    public string? OriginalData { get; set; }
    
    /// <summary>
    /// New data (JSON)
    /// </summary>
    [JsonPropertyName("newData")]
    public string? NewData { get; set; }
    
    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    [JsonPropertyName("metadata")]
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Operation duration in milliseconds
    /// </summary>
    [JsonPropertyName("durationMs")]
    public long? DurationMs { get; set; }
    
    /// <summary>
    /// Tags for categorization
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Indicates if contains sensitive data
    /// </summary>
    [JsonPropertyName("containsSensitiveData")]
    public bool ContainsSensitiveData { get; set; }
    
    /// <summary>
    /// Retention category for compliance
    /// </summary>
    [JsonPropertyName("retentionCategory")]
    public string? RetentionCategory { get; set; }
}
