using AuditService.Application.Commands;
using AuditService.Domain.Contracts;
using AuditService.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AuditService.Application.Handlers;

/// <summary>
/// Handler for ProcessAuditEventCommand
/// Converts audit events to audit logs and persists them
/// </summary>
public class ProcessAuditEventHandler : IRequestHandler<ProcessAuditEventCommand>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessAuditEventHandler> _logger;

    public ProcessAuditEventHandler(
        IAuditLogRepository repository,
        IMapper mapper,
        ILogger<ProcessAuditEventHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(ProcessAuditEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing audit event: {EventId} for service: {ServiceName}", 
                request.AuditEvent.EventId, request.AuditEvent.ServiceName);

            var auditLog = _mapper.Map<AuditLog>(request.AuditEvent);
            
            // Calculate data hash for integrity verification
            auditLog.DataHash = CalculateDataHash(auditLog);
            
            // Set expiry date based on retention policy
            auditLog.ExpiryDate = CalculateExpiryDate(auditLog);

            await _repository.AddAsync(auditLog, cancellationToken);

            _logger.LogInformation("Successfully processed audit event: {EventId}", request.AuditEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit event: {EventId}", request.AuditEvent.EventId);
            throw;
        }
    }

    private static string CalculateDataHash(AuditLog auditLog)
    {
        // Create a hash of key audit data for integrity verification
        var dataToHash = $"{auditLog.CorrelationId}|{auditLog.Timestamp:O}|{auditLog.ServiceName}|{auditLog.UserId}|{auditLog.ActionType}|{auditLog.EntityType}|{auditLog.EntityId}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        return Convert.ToBase64String(hashBytes);
    }

    private static DateTime? CalculateExpiryDate(AuditLog auditLog)
    {
        // Default retention periods based on category
        var retentionDays = auditLog.RetentionCategory?.ToLowerInvariant() switch
        {
            "security" => 2555, // 7 years for security events
            "financial" => 2555, // 7 years for financial records
            "hr" => 2190, // 6 years for HR records
            "compliance" => 3650, // 10 years for compliance
            "audit" => 2555, // 7 years for audit records
            "operational" => 365, // 1 year for operational logs
            _ => 1095 // 3 years default
        };

        return auditLog.Timestamp.AddDays(retentionDays);
    }
}

/// <summary>
/// Handler for ProcessAuditEventBatchCommand
/// Processes multiple audit events efficiently in batch
/// </summary>
public class ProcessAuditEventBatchHandler : IRequestHandler<ProcessAuditEventBatchCommand>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessAuditEventBatchHandler> _logger;

    public ProcessAuditEventBatchHandler(
        IAuditLogRepository repository,
        IMapper mapper,
        ILogger<ProcessAuditEventBatchHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(ProcessAuditEventBatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var eventCount = request.AuditEvents.Count();
            _logger.LogInformation("Processing batch of {Count} audit events", eventCount);

            var auditLogs = new List<AuditLog>();

            foreach (var auditEvent in request.AuditEvents)
            {
                var auditLog = _mapper.Map<AuditLog>(auditEvent);
                auditLog.DataHash = CalculateDataHash(auditLog);
                auditLog.ExpiryDate = CalculateExpiryDate(auditLog);
                auditLogs.Add(auditLog);
            }

            await _repository.AddRangeAsync(auditLogs, cancellationToken);

            _logger.LogInformation("Successfully processed batch of {Count} audit events", eventCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit event batch");
            throw;
        }
    }

    private static string CalculateDataHash(AuditLog auditLog)
    {
        var dataToHash = $"{auditLog.CorrelationId}|{auditLog.Timestamp:O}|{auditLog.ServiceName}|{auditLog.UserId}|{auditLog.ActionType}|{auditLog.EntityType}|{auditLog.EntityId}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        return Convert.ToBase64String(hashBytes);
    }

    private static DateTime? CalculateExpiryDate(AuditLog auditLog)
    {
        var retentionDays = auditLog.RetentionCategory?.ToLowerInvariant() switch
        {
            "security" => 2555,
            "financial" => 2555,
            "hr" => 2190,
            "compliance" => 3650,
            "audit" => 2555,
            "operational" => 365,
            _ => 1095
        };

        return auditLog.Timestamp.AddDays(retentionDays);
    }
}
