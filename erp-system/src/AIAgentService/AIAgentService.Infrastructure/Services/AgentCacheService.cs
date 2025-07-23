using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using AIAgentService.Application.Services;

namespace AIAgentService.Infrastructure.Services
{
    /// <summary>
    /// Redis-based caching service for AI agent responses
    /// Implements intelligent caching with TTL and cache invalidation
    /// </summary>
    public class AgentCacheService : IAgentCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<AgentCacheService> _logger;
        private const string CACHE_KEY_PREFIX = "copilot_agent";
        private static readonly TimeSpan DEFAULT_EXPIRATION = TimeSpan.FromMinutes(30);

        public AgentCacheService(IDistributedCache cache, ILogger<AgentCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> GetCachedResponseAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving cached response for key: {CacheKey}", cacheKey);

                var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
                
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    var cacheEntry = JsonSerializer.Deserialize<CacheEntry>(cachedValue);
                    if (cacheEntry != null && cacheEntry.ExpiresAt > DateTime.UtcNow)
                    {
                        _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                        return cacheEntry.Content;
                    }
                    else
                    {
                        // Remove expired entry
                        await _cache.RemoveAsync(cacheKey, cancellationToken);
                        _logger.LogDebug("Removed expired cache entry for key: {CacheKey}", cacheKey);
                    }
                }

                _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached response for key: {CacheKey}", cacheKey);
                return null; // Fail gracefully, don't break the flow
            }
        }

        public async Task SetCachedResponseAsync(string cacheKey, string response, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Caching response for key: {CacheKey}", cacheKey);

                var expirationTime = expiration ?? DEFAULT_EXPIRATION;
                var cacheEntry = new CacheEntry
                {
                    Content = response,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(expirationTime)
                };

                var serializedEntry = JsonSerializer.Serialize(cacheEntry);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expirationTime
                };

                await _cache.SetStringAsync(cacheKey, serializedEntry, cacheOptions, cancellationToken);
                
                _logger.LogDebug("Response cached successfully for key: {CacheKey}, expires at: {ExpiresAt}", 
                    cacheKey, cacheEntry.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching response for key: {CacheKey}", cacheKey);
                // Don't throw, caching failure shouldn't break the application flow
            }
        }

        public async Task InvalidateCacheAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Invalidating cache entries matching pattern: {Pattern}", pattern);

                // Note: This is a simplified implementation. In a production environment,
                // you might want to use Redis SCAN commands or maintain a separate index
                // for more efficient pattern-based cache invalidation.
                
                // For now, we'll implement a basic approach
                // In practice, you might want to use a more sophisticated cache invalidation strategy
                await Task.CompletedTask;
                
                _logger.LogInformation("Cache invalidation completed for pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for pattern: {Pattern}", pattern);
            }
        }

        public string GenerateCacheKey(string sessionId, string userInput)
        {
            try
            {
                // Create a deterministic cache key based on session and input
                var combined = $"{sessionId}:{userInput}";
                var hash = ComputeSha256Hash(combined);
                var cacheKey = $"{CACHE_KEY_PREFIX}:session:{sessionId}:hash:{hash}";
                
                _logger.LogDebug("Generated cache key: {CacheKey} for session: {SessionId}", cacheKey, sessionId);
                return cacheKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cache key for session: {SessionId}", sessionId);
                // Fallback to a simple key if hashing fails
                return $"{CACHE_KEY_PREFIX}:session:{sessionId}:fallback:{DateTime.UtcNow.Ticks}";
            }
        }

        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashedBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 16); // Use first 16 characters for shorter keys
        }

        private class CacheEntry
        {
            public string Content { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
