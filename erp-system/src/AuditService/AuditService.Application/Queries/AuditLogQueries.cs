using AuditService.Application.DTOs;
using MediatR;

namespace AuditService.Application.Queries;

/// <summary>
/// Query to retrieve audit logs with filtering and pagination
/// </summary>
public class GetAuditLogsQuery : IRequest<PagedAuditLogDto>
{
    public AuditLogQueryDto QueryParameters { get; }
    
    public GetAuditLogsQuery(AuditLogQueryDto queryParameters)
    {
        QueryParameters = queryParameters;
    }
}

/// <summary>
/// Query to retrieve audit logs by correlation ID
/// </summary>
public class GetAuditLogsByCorrelationIdQuery : IRequest<IEnumerable<AuditLogDto>>
{
    public string CorrelationId { get; }
    
    public GetAuditLogsByCorrelationIdQuery(string correlationId)
    {
        CorrelationId = correlationId;
    }
}

/// <summary>
/// Query to retrieve audit logs for a specific entity
/// </summary>
public class GetAuditLogsByEntityQuery : IRequest<IEnumerable<AuditLogDto>>
{
    public string EntityId { get; }
    public string EntityType { get; }
    
    public GetAuditLogsByEntityQuery(string entityId, string entityType)
    {
        EntityId = entityId;
        EntityType = entityType;
    }
}
