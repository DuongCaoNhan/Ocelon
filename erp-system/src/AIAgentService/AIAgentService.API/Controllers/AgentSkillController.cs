using Microsoft.AspNetCore.Mvc;
using MediatR;
using AIAgentService.Application.Commands;
using AIAgentService.Application.Queries;
using AIAgentService.Application.DTOs;

namespace AIAgentService.API.Controllers
{
    /// <summary>
    /// Controller for managing AI agent skills and capabilities
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AgentSkillController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AgentSkillController> _logger;

        public AgentSkillController(IMediator mediator, ILogger<AgentSkillController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all agent skills with optional filtering
        /// </summary>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="serviceName">Filter by service name</param>
        /// <param name="skillType">Filter by skill type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of agent skills</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AgentSkillDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AgentSkillDto>>> GetSkills(
            [FromQuery] bool? isActive = null,
            [FromQuery] string? serviceName = null,
            [FromQuery] string? skillType = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent skills with filters - Active: {IsActive}, Service: {ServiceName}, Type: {SkillType}", 
                    isActive, serviceName, skillType);

                var query = new GetAgentSkillsQuery
                {
                    IsActive = isActive,
                    ServiceName = serviceName,
                    SkillType = skillType
                };

                var result = await _mediator.Send(query, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skills");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving skills");
            }
        }

        /// <summary>
        /// Get a specific agent skill by ID
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Skill details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AgentSkillDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AgentSkillDto>> GetSkill(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent skill {SkillId}", id);

                var query = new GetAgentSkillByIdQuery { SkillId = id };
                var result = await _mediator.Send(query, cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Agent skill {SkillId} not found", id);
                    return NotFound($"Skill {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skill {SkillId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving the skill");
            }
        }

        /// <summary>
        /// Get available skill names for the agent
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available skill names</returns>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetAvailableSkills(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving available skills for agent");

                var query = new GetAvailableSkillsQuery();
                var result = await _mediator.Send(query, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available skills");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving available skills");
            }
        }

        /// <summary>
        /// Create a new agent skill
        /// </summary>
        /// <param name="command">Skill creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created skill details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AgentSkillDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AgentSkillDto>> CreateSkill(
            [FromBody] CreateAgentSkillCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating new agent skill {SkillName}", command.Name);

                var result = await _mediator.Send(command, cancellationToken);
                
                _logger.LogInformation("Agent skill {SkillName} created successfully with ID {SkillId}", 
                    result.Name, result.Id);
                
                return CreatedAtAction(
                    nameof(GetSkill), 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agent skill {SkillName}", command.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while creating the skill");
            }
        }

        /// <summary>
        /// Update an existing agent skill
        /// </summary>
        /// <param name="id">Skill ID</param>
        /// <param name="command">Skill update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated skill details</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AgentSkillDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AgentSkillDto>> UpdateSkill(
            Guid id,
            [FromBody] UpdateAgentSkillCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (command.Id != id)
                {
                    _logger.LogWarning("Skill ID mismatch in update request - URL: {UrlId}, Body: {BodyId}", id, command.Id);
                    return BadRequest("Skill ID mismatch");
                }

                _logger.LogInformation("Updating agent skill {SkillId}", id);

                var result = await _mediator.Send(command, cancellationToken);
                
                _logger.LogInformation("Agent skill {SkillId} updated successfully", id);
                
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Agent skill {SkillId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent skill {SkillId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while updating the skill");
            }
        }
    }
}
