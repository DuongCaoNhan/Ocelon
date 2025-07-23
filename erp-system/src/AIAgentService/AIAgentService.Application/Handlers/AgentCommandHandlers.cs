using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AIAgentService.Application.Commands;
using AIAgentService.Application.DTOs;
using AIAgentService.Domain.Entities;
using AIAgentService.Domain.Repositories;
using AIAgentService.Domain.Services;

namespace AIAgentService.Application.Handlers
{
    public class CreateAgentSessionHandler : IRequestHandler<CreateAgentSessionCommand, AgentSessionDto>
    {
        private readonly IAgentSessionRepository _sessionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateAgentSessionHandler> _logger;

        public CreateAgentSessionHandler(
            IAgentSessionRepository sessionRepository,
            IMapper mapper,
            ILogger<CreateAgentSessionHandler> logger)
        {
            _sessionRepository = sessionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AgentSessionDto> Handle(CreateAgentSessionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new agent session for user {UserId}", request.UserId);

                var session = new AgentSession(
                    request.UserId,
                    request.SessionName,
                    request.Description,
                    request.Context);

                var createdSession = await _sessionRepository.AddAsync(session, cancellationToken);
                
                _logger.LogInformation("Agent session {SessionId} created successfully", createdSession.Id);
                
                return _mapper.Map<AgentSessionDto>(createdSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agent session for user {UserId}", request.UserId);
                throw;
            }
        }
    }

    public class EndAgentSessionHandler : IRequestHandler<EndAgentSessionCommand, bool>
    {
        private readonly IAgentSessionRepository _sessionRepository;
        private readonly ILogger<EndAgentSessionHandler> _logger;

        public EndAgentSessionHandler(
            IAgentSessionRepository sessionRepository,
            ILogger<EndAgentSessionHandler> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(EndAgentSessionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ending agent session {SessionId}", request.SessionId);

                var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
                if (session == null)
                {
                    _logger.LogWarning("Agent session {SessionId} not found", request.SessionId);
                    return false;
                }

                session.EndSession();
                await _sessionRepository.UpdateAsync(session, cancellationToken);
                
                _logger.LogInformation("Agent session {SessionId} ended successfully", request.SessionId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending agent session {SessionId}", request.SessionId);
                throw;
            }
        }
    }

    public class SendMessageToAgentHandler : IRequestHandler<SendMessageToAgentCommand, AgentMessageDto>
    {
        private readonly IAgentSessionRepository _sessionRepository;
        private readonly IAgentOrchestrationService _orchestrationService;
        private readonly IMapper _mapper;
        private readonly ILogger<SendMessageToAgentHandler> _logger;

        public SendMessageToAgentHandler(
            IAgentSessionRepository sessionRepository,
            IAgentOrchestrationService orchestrationService,
            IMapper mapper,
            ILogger<SendMessageToAgentHandler> logger)
        {
            _sessionRepository = sessionRepository;
            _orchestrationService = orchestrationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AgentMessageDto> Handle(SendMessageToAgentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing message for session {SessionId}", request.SessionId);

                var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
                if (session == null)
                {
                    throw new InvalidOperationException($"Session {request.SessionId} not found");
                }

                // Add user message
                session.AddMessage(request.Content, MessageType.User, request.Metadata);

                // Process with AI agent
                var agentResponse = await _orchestrationService.ProcessUserRequestAsync(
                    request.SessionId.ToString(), 
                    request.Content, 
                    cancellationToken);

                // Add agent response
                session.AddMessage(agentResponse, MessageType.Agent);

                await _sessionRepository.UpdateAsync(session, cancellationToken);

                // Return the agent's message
                var agentMessage = session.Messages.Last(m => m.MessageType == MessageType.Agent);
                
                _logger.LogInformation("Message processed successfully for session {SessionId}", request.SessionId);
                
                return _mapper.Map<AgentMessageDto>(agentMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for session {SessionId}", request.SessionId);
                throw;
            }
        }
    }
}
