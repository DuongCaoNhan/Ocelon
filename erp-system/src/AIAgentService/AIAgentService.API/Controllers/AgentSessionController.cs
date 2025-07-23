using Microsoft.AspNetCore.Mvc;
using MediatR;
using AIAgentService.Application.Commands;
using AIAgentService.Application.Queries;
using AIAgentService.Application.DTOs;

namespace AIAgentService.API.Controllers
{
    /// <summary>
    /// Controller for managing AI agent sessions and interactions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AgentSessionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AgentSessionController> _logger;

        public AgentSessionController(IMediator mediator, ILogger<AgentSessionController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new agent session
        /// </summary>
        /// <param name="command">Session creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created session details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AgentSessionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AgentSessionDto>> CreateSession(
            [FromBody] CreateAgentSessionCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating new agent session for user {UserId}", command.UserId);

                var result = await _mediator.Send(command, cancellationToken);
                
                _logger.LogInformation("Agent session {SessionId} created successfully", result.Id);
                
                return CreatedAtAction(
                    nameof(GetSession), 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agent session for user {UserId}", command.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while creating the session");
            }
        }

        /// <summary>
        /// Get an agent session by ID
        /// </summary>
        /// <param name="id">Session ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Session details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AgentSessionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AgentSessionDto>> GetSession(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent session {SessionId}", id);

                var query = new GetAgentSessionByIdQuery { SessionId = id };
                var result = await _mediator.Send(query, cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Agent session {SessionId} not found", id);
                    return NotFound($"Session {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent session {SessionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving the session");
            }
        }

        /// <summary>
        /// Get agent sessions for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of user sessions</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<AgentSessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AgentSessionDto>>> GetUserSessions(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent sessions for user {UserId}", userId);

                var query = new GetAgentSessionsByUserQuery { UserId = userId };
                var result = await _mediator.Send(query, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent sessions for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving user sessions");
            }
        }

        /// <summary>
        /// Get all active agent sessions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of active sessions</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<AgentSessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AgentSessionDto>>> GetActiveSessions(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving active agent sessions");

                var query = new GetActiveAgentSessionsQuery();
                var result = await _mediator.Send(query, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active agent sessions");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving active sessions");
            }
        }

        /// <summary>
        /// Send a message to the AI agent
        /// </summary>
        /// <param name="command">Message details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Agent response</returns>
        [HttpPost("message")]
        [ProducesResponseType(typeof(AgentMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AgentMessageDto>> SendMessage(
            [FromBody] SendMessageToAgentCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing message for session {SessionId}", command.SessionId);

                var result = await _mediator.Send(command, cancellationToken);
                
                _logger.LogInformation("Message processed successfully for session {SessionId}", command.SessionId);
                
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Session {SessionId} not found or invalid", command.SessionId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for session {SessionId}", command.SessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing the message");
            }
        }

        /// <summary>
        /// End an agent session
        /// </summary>
        /// <param name="id">Session ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpPut("{id:guid}/end")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> EndSession(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Ending agent session {SessionId}", id);

                var command = new EndAgentSessionCommand { SessionId = id };
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                {
                    _logger.LogWarning("Agent session {SessionId} not found", id);
                    return NotFound($"Session {id} not found");
                }

                _logger.LogInformation("Agent session {SessionId} ended successfully", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending agent session {SessionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while ending the session");
            }
        }
    }
}
