namespace AIAgentService.Domain.Services
{
    /// <summary>
    /// Domain service for managing agent interactions and orchestration
    /// </summary>
    public interface IAgentOrchestrationService
    {
        Task<string> ProcessUserRequestAsync(string sessionId, string userMessage, CancellationToken cancellationToken = default);
        Task<bool> ValidateSkillExecutionAsync(string skillName, string parameters, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetAvailableSkillsAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateResponseAsync(string context, string userInput, CancellationToken cancellationToken = default);
    }
}
