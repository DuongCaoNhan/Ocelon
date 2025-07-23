using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AIAgentService.Application.Services
{
    /// <summary>
    /// Service for managing Semantic Kernel skills and AI interactions
    /// </summary>
    public interface ISemanticKernelService
    {
        Task<string> GenerateResponseAsync(string userInput, string? context = null, CancellationToken cancellationToken = default);
        Task<string> ExecuteSkillAsync(string skillName, string parameters, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetAvailableSkillsAsync(CancellationToken cancellationToken = default);
        Task RegisterSkillAsync(string skillName, string skillConfiguration, CancellationToken cancellationToken = default);
        Task<bool> ValidateSkillAsync(string skillName, string parameters, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for integrating with external ERP services
    /// </summary>
    public interface IExternalServiceIntegration
    {
        Task<string> CallHRServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default);
        Task<string> CallInventoryServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default);
        Task<string> CallAccountingServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default);
        Task<string> CallWorkflowServiceAsync(string operation, string parameters, CancellationToken cancellationToken = default);
        Task<bool> IsServiceAvailableAsync(string serviceName, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for caching and optimizing agent responses
    /// </summary>
    public interface IAgentCacheService
    {
        Task<string?> GetCachedResponseAsync(string cacheKey, CancellationToken cancellationToken = default);
        Task SetCachedResponseAsync(string cacheKey, string response, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task InvalidateCacheAsync(string pattern, CancellationToken cancellationToken = default);
        string GenerateCacheKey(string sessionId, string userInput);
    }
}
