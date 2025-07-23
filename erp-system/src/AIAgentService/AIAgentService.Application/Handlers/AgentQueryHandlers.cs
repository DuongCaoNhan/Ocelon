using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AIAgentService.Application.Queries;
using AIAgentService.Application.DTOs;
using AIAgentService.Domain.Repositories;
using AIAgentService.Domain.Services;

namespace AIAgentService.Application.Handlers
{
    public class GetAgentSessionByIdHandler : IRequestHandler<GetAgentSessionByIdQuery, AgentSessionDto?>
    {
        private readonly IAgentSessionRepository _sessionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAgentSessionByIdHandler> _logger;

        public GetAgentSessionByIdHandler(
            IAgentSessionRepository sessionRepository,
            IMapper mapper,
            ILogger<GetAgentSessionByIdHandler> logger)
        {
            _sessionRepository = sessionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AgentSessionDto?> Handle(GetAgentSessionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving agent session {SessionId}", request.SessionId);

                var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
                
                return session != null ? _mapper.Map<AgentSessionDto>(session) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent session {SessionId}", request.SessionId);
                throw;
            }
        }
    }

    public class GetAgentSessionsByUserHandler : IRequestHandler<GetAgentSessionsByUserQuery, IEnumerable<AgentSessionDto>>
    {
        private readonly IAgentSessionRepository _sessionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAgentSessionsByUserHandler> _logger;

        public GetAgentSessionsByUserHandler(
            IAgentSessionRepository sessionRepository,
            IMapper mapper,
            ILogger<GetAgentSessionsByUserHandler> logger)
        {
            _sessionRepository = sessionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AgentSessionDto>> Handle(GetAgentSessionsByUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving agent sessions for user {UserId}", request.UserId);

                var sessions = await _sessionRepository.GetByUserIdAsync(request.UserId, cancellationToken);
                
                return _mapper.Map<IEnumerable<AgentSessionDto>>(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent sessions for user {UserId}", request.UserId);
                throw;
            }
        }
    }

    public class GetAgentSkillsHandler : IRequestHandler<GetAgentSkillsQuery, IEnumerable<AgentSkillDto>>
    {
        private readonly IAgentSkillRepository _skillRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAgentSkillsHandler> _logger;

        public GetAgentSkillsHandler(
            IAgentSkillRepository skillRepository,
            IMapper mapper,
            ILogger<GetAgentSkillsHandler> logger)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AgentSkillDto>> Handle(GetAgentSkillsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving agent skills with filters");

                IEnumerable<Domain.Entities.AgentSkill> skills;

                if (request.IsActive.HasValue && request.IsActive.Value)
                {
                    skills = await _skillRepository.GetActiveSkillsAsync(cancellationToken);
                }
                else if (!string.IsNullOrEmpty(request.ServiceName))
                {
                    skills = await _skillRepository.GetByServiceNameAsync(request.ServiceName, cancellationToken);
                }
                else if (!string.IsNullOrEmpty(request.SkillType))
                {
                    skills = await _skillRepository.GetBySkillTypeAsync(request.SkillType, cancellationToken);
                }
                else
                {
                    skills = await _skillRepository.GetActiveSkillsAsync(cancellationToken);
                }

                return _mapper.Map<IEnumerable<AgentSkillDto>>(skills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skills");
                throw;
            }
        }
    }

    public class GetAvailableSkillsHandler : IRequestHandler<GetAvailableSkillsQuery, IEnumerable<string>>
    {
        private readonly IAgentOrchestrationService _orchestrationService;
        private readonly ILogger<GetAvailableSkillsHandler> _logger;

        public GetAvailableSkillsHandler(
            IAgentOrchestrationService orchestrationService,
            ILogger<GetAvailableSkillsHandler> logger)
        {
            _orchestrationService = orchestrationService;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> Handle(GetAvailableSkillsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving available skills for agent");

                return await _orchestrationService.GetAvailableSkillsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available skills");
                throw;
            }
        }
    }
}
