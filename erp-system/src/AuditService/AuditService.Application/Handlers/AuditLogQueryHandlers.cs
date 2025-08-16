using AuditService.Application.DTOs;
using AuditService.Application.Queries;
using AuditService.Domain.Contracts;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuditService.Application.Handlers;

/// <summary>
/// Handler for GetAuditLogsQuery
/// Retrieves audit logs with filtering and pagination
/// </summary>
public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, PagedAuditLogDto>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuditLogsHandler> _logger;

    public GetAuditLogsHandler(
        IAuditLogRepository repository,
        IMapper mapper,
        ILogger<GetAuditLogsHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedAuditLogDto> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = request.QueryParameters;
            
            _logger.LogInformation("Retrieving audit logs with query: {@Query}", query);

            var (logs, totalCount) = await _repository.GetAuditLogsAsync(
                userId: query.UserId,
                serviceName: query.ServiceName,
                entityType: query.EntityType,
                action: query.ActionType?.ToString(),
                fromDate: query.FromDate,
                toDate: query.ToDate,
                pageNumber: query.PageNumber,
                pageSize: query.PageSize,
                cancellationToken: cancellationToken);

            var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);

            var result = new PagedAuditLogDto
            {
                Logs = auditLogDtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            _logger.LogInformation("Retrieved {Count} audit logs out of {Total} total", 
                auditLogDtos.Count(), totalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            throw;
        }
    }
}

/// <summary>
/// Handler for GetAuditLogsByCorrelationIdQuery
/// Retrieves audit logs by correlation ID for request tracing
/// </summary>
public class GetAuditLogsByCorrelationIdHandler : IRequestHandler<GetAuditLogsByCorrelationIdQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuditLogsByCorrelationIdHandler> _logger;

    public GetAuditLogsByCorrelationIdHandler(
        IAuditLogRepository repository,
        IMapper mapper,
        ILogger<GetAuditLogsByCorrelationIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetAuditLogsByCorrelationIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving audit logs for correlation ID: {CorrelationId}", request.CorrelationId);

            var logs = await _repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
            var result = _mapper.Map<IEnumerable<AuditLogDto>>(logs);

            _logger.LogInformation("Retrieved {Count} audit logs for correlation ID: {CorrelationId}", 
                result.Count(), request.CorrelationId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for correlation ID: {CorrelationId}", request.CorrelationId);
            throw;
        }
    }
}

/// <summary>
/// Handler for GetAuditLogsByEntityQuery
/// Retrieves audit logs for a specific entity
/// </summary>
public class GetAuditLogsByEntityHandler : IRequestHandler<GetAuditLogsByEntityQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuditLogsByEntityHandler> _logger;

    public GetAuditLogsByEntityHandler(
        IAuditLogRepository repository,
        IMapper mapper,
        ILogger<GetAuditLogsByEntityHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetAuditLogsByEntityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving audit logs for entity: {EntityType}/{EntityId}", 
                request.EntityType, request.EntityId);

            var logs = await _repository.GetByEntityAsync(request.EntityId, request.EntityType, cancellationToken);
            var result = _mapper.Map<IEnumerable<AuditLogDto>>(logs);

            _logger.LogInformation("Retrieved {Count} audit logs for entity: {EntityType}/{EntityId}", 
                result.Count(), request.EntityType, request.EntityId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity: {EntityType}/{EntityId}", 
                request.EntityType, request.EntityId);
            throw;
        }
    }
}
