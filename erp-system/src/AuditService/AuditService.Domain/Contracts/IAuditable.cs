namespace AuditService.Domain.Contracts;

/// <summary>
/// Interface for entities that require audit tracking
/// Automatically captures creation and modification metadata
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// User or service that created the entity
    /// </summary>
    string CreatedBy { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// User or service that last updated the entity
    /// </summary>
    string? UpdatedBy { get; set; }
}
