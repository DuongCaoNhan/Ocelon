using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using AIAgentService.Application.Services;
using AIAgentService.Domain.Services;
using AIAgentService.Domain.Repositories;

namespace AIAgentService.Infrastructure.Services
{
    /// <summary>
    /// Implementation of AI agent orchestration using Semantic Kernel
    /// Integrates with Azure OpenAI and coordinates skill execution
    /// </summary>
    public class SemanticKernelOrchestrationService : IAgentOrchestrationService, ISemanticKernelService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletion;
        private readonly IAgentSkillRepository _skillRepository;
        private readonly IExternalServiceIntegration _externalServices;
        private readonly IAgentCacheService _cacheService;
        private readonly ILogger<SemanticKernelOrchestrationService> _logger;
        private readonly IConfiguration _configuration;

        public SemanticKernelOrchestrationService(
            Kernel kernel,
            IChatCompletionService chatCompletion,
            IAgentSkillRepository skillRepository,
            IExternalServiceIntegration externalServices,
            IAgentCacheService cacheService,
            ILogger<SemanticKernelOrchestrationService> logger,
            IConfiguration configuration)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _chatCompletion = chatCompletion ?? throw new ArgumentNullException(nameof(chatCompletion));
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
            _externalServices = externalServices ?? throw new ArgumentNullException(nameof(externalServices));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<string> ProcessUserRequestAsync(string sessionId, string userMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing user request for session {SessionId}", sessionId);

                // Check cache first
                var cacheKey = _cacheService.GenerateCacheKey(sessionId, userMessage);
                var cachedResponse = await _cacheService.GetCachedResponseAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedResponse))
                {
                    _logger.LogDebug("Returning cached response for session {SessionId}", sessionId);
                    return cachedResponse;
                }

                // Build conversation context
                var systemPrompt = BuildSystemPrompt();
                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage(userMessage);

                // Generate response using AI
                var response = await _chatCompletion.GetChatMessageContentAsync(
                    chatHistory,
                    new OpenAIPromptExecutionSettings
                    {
                        MaxTokens = 1000,
                        Temperature = 0.7,
                        TopP = 0.95
                    },
                    _kernel,
                    cancellationToken);

                var responseContent = response.Content ?? "I apologize, but I couldn't generate a response at this time.";

                // Cache the response
                await _cacheService.SetCachedResponseAsync(
                    cacheKey, 
                    responseContent, 
                    TimeSpan.FromMinutes(15), 
                    cancellationToken);

                _logger.LogInformation("User request processed successfully for session {SessionId}", sessionId);
                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user request for session {SessionId}", sessionId);
                return "I apologize, but I encountered an error while processing your request. Please try again.";
            }
        }

        public async Task<string> GenerateResponseAsync(string userInput, string? context = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Generating AI response with context");

                var systemPrompt = $"{BuildSystemPrompt()}\n\nContext: {context}";
                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage(userInput);

                var response = await _chatCompletion.GetChatMessageContentAsync(
                    chatHistory,
                    new OpenAIPromptExecutionSettings
                    {
                        MaxTokens = 1000,
                        Temperature = 0.7
                    },
                    _kernel,
                    cancellationToken);

                return response.Content ?? "I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response");
                throw;
            }
        }

        public async Task<bool> ValidateSkillExecutionAsync(string skillName, string parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Validating skill execution for {SkillName}", skillName);

                var skill = await _skillRepository.GetByNameAsync(skillName, cancellationToken);
                if (skill == null || !skill.IsActive)
                {
                    return false;
                }

                // Validate that the external service is available
                return await _externalServices.IsServiceAvailableAsync(skill.ServiceName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating skill execution for {SkillName}", skillName);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetAvailableSkillsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving available skills");

                var skills = await _skillRepository.GetActiveSkillsAsync(cancellationToken);
                return skills.Select(s => s.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available skills");
                throw;
            }
        }

        public async Task<string> ExecuteSkillAsync(string skillName, string parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Executing skill {SkillName}", skillName);

                var skill = await _skillRepository.GetByNameAsync(skillName, cancellationToken);
                if (skill == null || !skill.IsActive)
                {
                    throw new InvalidOperationException($"Skill '{skillName}' not found or inactive");
                }

                // Route to appropriate service based on skill configuration
                return skill.ServiceName.ToLowerInvariant() switch
                {
                    "hrservice" => await _externalServices.CallHRServiceAsync(skillName, parameters, cancellationToken),
                    "inventoryservice" => await _externalServices.CallInventoryServiceAsync(skillName, parameters, cancellationToken),
                    "accountingservice" => await _externalServices.CallAccountingServiceAsync(skillName, parameters, cancellationToken),
                    "workflowservice" => await _externalServices.CallWorkflowServiceAsync(skillName, parameters, cancellationToken),
                    _ => throw new NotSupportedException($"Service '{skill.ServiceName}' not supported")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing skill {SkillName}", skillName);
                throw;
            }
        }

        public async Task RegisterSkillAsync(string skillName, string skillConfiguration, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Registering skill {SkillName}", skillName);

                // Implementation would register skills with Semantic Kernel
                // This is a placeholder for future enhancement
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering skill {SkillName}", skillName);
                throw;
            }
        }

        public async Task<bool> ValidateSkillAsync(string skillName, string parameters, CancellationToken cancellationToken = default)
        {
            return await ValidateSkillExecutionAsync(skillName, parameters, cancellationToken);
        }

        private string BuildSystemPrompt()
        {
            return @"You are an intelligent ERP assistant that helps users with business operations and workflows. 
You have access to various services including HR, Inventory, Accounting, and Workflow management.

Your capabilities include:
- Answering questions about business processes
- Helping with data analysis and reporting
- Providing recommendations for operational improvements
- Assisting with task automation and workflow optimization
- Offering insights based on system data

Always be helpful, professional, and provide accurate information. If you're unsure about something, 
ask for clarification rather than making assumptions. When possible, suggest specific actions 
the user can take to achieve their goals.";
        }
    }
}
