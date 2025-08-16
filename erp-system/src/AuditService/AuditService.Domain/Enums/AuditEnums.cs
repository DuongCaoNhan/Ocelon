using System.Text.Json.Serialization;

namespace AuditService.Domain.Enums;

/// <summary>
/// Enumeration of audit action types
/// Maps to common CRUD and business operations
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditActionType
{
    /// <summary>
    /// Entity creation operation
    /// </summary>
    Create,
    
    /// <summary>
    /// Entity read/view operation
    /// </summary>
    Read,
    
    /// <summary>
    /// Entity update/modification operation
    /// </summary>
    Update,
    
    /// <summary>
    /// Entity deletion operation
    /// </summary>
    Delete,
    
    /// <summary>
    /// User login operation
    /// </summary>
    Login,
    
    /// <summary>
    /// User logout operation
    /// </summary>
    Logout,
    
    /// <summary>
    /// Data export operation
    /// </summary>
    Export,
    
    /// <summary>
    /// Data import operation
    /// </summary>
    Import,
    
    /// <summary>
    /// Permission or access change
    /// </summary>
    AccessChange,
    
    /// <summary>
    /// Configuration change
    /// </summary>
    ConfigurationChange,
    
    /// <summary>
    /// Business process execution
    /// </summary>
    ProcessExecution,
    
    /// <summary>
    /// Failed operation attempt
    /// </summary>
    Failed,
    
    /// <summary>
    /// Other/custom operation
    /// </summary>
    Other
}

/// <summary>
/// Enumeration of audit result status
/// Indicates the outcome of the audited operation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditResult
{
    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Success,
    
    /// <summary>
    /// Operation failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Operation was denied due to authorization
    /// </summary>
    Denied,
    
    /// <summary>
    /// Operation was denied due to validation
    /// </summary>
    ValidationFailed,
    
    /// <summary>
    /// Operation timed out
    /// </summary>
    Timeout,
    
    /// <summary>
    /// Operation was cancelled
    /// </summary>
    Cancelled
}

/// <summary>
/// Enumeration of audit severity levels
/// Used for filtering and alerting
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditSeverity
{
    /// <summary>
    /// Informational audit entry
    /// </summary>
    Information,
    
    /// <summary>
    /// Warning - may require attention
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error - operation failed
    /// </summary>
    Error,
    
    /// <summary>
    /// Critical - security or compliance concern
    /// </summary>
    Critical
}
