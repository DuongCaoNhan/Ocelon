using AuditService.Domain.Contracts;
using AuditService.Domain.Entities;
using AuditService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuditService.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of IAuditLogRepository
/// Optimized for high-volume audit logging with efficient querying
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _context;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(AuditDbContext context, ILogger<AuditLogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added audit log: {AuditLogId}", auditLog.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding audit log: {AuditLogId}", auditLog.Id);
            throw;
        }
    }

    public async Task AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogList = auditLogs.ToList();
            _context.AuditLogs.AddRange(auditLogList);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added {Count} audit logs", auditLogList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding audit log batch");
            throw;
        }
    }

    public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(
        string? userId = null,
        string? serviceName = null,
        string? entityType = null,
        string? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.AuditLogs.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(x => x.UserId == userId);
            }

            if (!string.IsNullOrEmpty(serviceName))
            {
                query = query.Where(x => x.ServiceName == serviceName);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(x => x.EntityType == entityType);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(x => x.ActionType.ToString() == action);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.Timestamp <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var logs = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} audit logs out of {Total} total", logs.Count, totalCount);

            return (logs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            throw;
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(x => x.CorrelationId == correlationId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} audit logs for correlation ID: {CorrelationId}", logs.Count, correlationId);

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for correlation ID: {CorrelationId}", correlationId);
            throw;
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(x => x.EntityId == entityId && x.EntityType == entityType)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} audit logs for entity: {EntityType}/{EntityId}", logs.Count, entityType, entityId);

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity: {EntityType}/{EntityId}", entityType, entityId);
            throw;
        }
    }
}
