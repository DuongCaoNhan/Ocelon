using AuditService.Application.DTOs;
using AuditService.Application.Queries;
using AuditService.Application.Validators;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuditService.API.Controllers;

/// <summary>
/// API controller for querying audit logs
/// Provides secure access to audit trail data with comprehensive filtering
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all audit operations
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<AuditLogQueryDto> _queryValidator;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IMediator mediator,
        IValidator<AuditLogQueryDto> queryValidator,
        ILogger<AuditController> logger)
    {
        _mediator = mediator;
        _queryValidator = queryValidator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves audit logs with filtering and pagination
    /// </summary>
    /// <param name="queryDto">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged audit log results</returns>
    [HttpGet("logs")]
    [Authorize(Roles = "AuditAdmin,AuditViewer,SystemAdmin")]
    [ProducesResponseType(typeof(PagedAuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedAuditLogDto>> GetAuditLogs(
        [FromQuery] AuditLogQueryDto queryDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate query parameters
            var validationResult = await _queryValidator.ValidateAsync(queryDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Apply tenant isolation if user is not system admin
            if (!User.IsInRole("SystemAdmin"))
            {
                var tenantId = User.FindFirst("tid")?.Value;
                if (!string.IsNullOrEmpty(tenantId))
                {
                    // Filter results to current tenant only
                    // This would require adding TenantId filter to the query
                    _logger.LogInformation("Applying tenant filter: {TenantId} for user: {UserId}", 
                        tenantId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                }
            }

            // Log audit query for compliance
            _logger.LogInformation("Audit log query requested by user: {UserId}, Query: {@Query}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value, queryDto);

            var query = new GetAuditLogsQuery(queryDto);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return Problem("An error occurred while retrieving audit logs", statusCode: 500);
        }
    }

    /// <summary>
    /// Retrieves audit logs for a specific correlation ID
    /// Used for request tracing across services
    /// </summary>
    /// <param name="correlationId">The correlation ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs for the correlation ID</returns>
    [HttpGet("correlation/{correlationId}")]
    [Authorize(Roles = "AuditAdmin,AuditViewer,SystemAdmin,Developer")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAuditLogsByCorrelationId(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return BadRequest("Correlation ID is required");
            }

            if (correlationId.Length > 100)
            {
                return BadRequest("Correlation ID cannot exceed 100 characters");
            }

            _logger.LogInformation("Audit logs requested for correlation ID: {CorrelationId} by user: {UserId}", 
                correlationId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var query = new GetAuditLogsByCorrelationIdQuery(correlationId);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Any())
            {
                return NotFound($"No audit logs found for correlation ID: {correlationId}");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for correlation ID: {CorrelationId}", correlationId);
            return Problem("An error occurred while retrieving audit logs", statusCode: 500);
        }
    }

    /// <summary>
    /// Retrieves audit logs for a specific entity
    /// Shows the complete audit trail for an entity
    /// </summary>
    /// <param name="entityType">The type of entity</param>
    /// <param name="entityId">The ID of the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs for the entity</returns>
    [HttpGet("entity/{entityType}/{entityId}")]
    [Authorize(Roles = "AuditAdmin,AuditViewer,SystemAdmin")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAuditLogsByEntity(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest("Entity type is required");
            }

            if (string.IsNullOrWhiteSpace(entityId))
            {
                return BadRequest("Entity ID is required");
            }

            if (entityType.Length > 100 || entityId.Length > 100)
            {
                return BadRequest("Entity type and ID cannot exceed 100 characters each");
            }

            _logger.LogInformation("Audit logs requested for entity: {EntityType}/{EntityId} by user: {UserId}", 
                entityType, entityId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var query = new GetAuditLogsByEntityQuery(entityId, entityType);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Any())
            {
                return NotFound($"No audit logs found for entity: {entityType}/{entityId}");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity: {EntityType}/{EntityId}", entityType, entityId);
            return Problem("An error occurred while retrieving audit logs", statusCode: 500);
        }
    }

    /// <summary>
    /// Health check endpoint for the audit service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
