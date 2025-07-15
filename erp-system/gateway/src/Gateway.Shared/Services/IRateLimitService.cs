namespace Gateway.Shared.Services;

/// <summary>
/// Interface for rate limiting service
/// </summary>
public interface IRateLimitService
{
    Task<bool> IsAllowedAsync(string identifier, int maxRequests = 100, TimeSpan? timeWindow = null);
}

/// <summary>
/// Basic in-memory rate limiting implementation
/// </summary>
public class BasicRateLimitService : IRateLimitService
{
    private readonly Dictionary<string, (DateTime LastReset, int RequestCount)> _requestCounts = new();
    private readonly object _lock = new();

    public Task<bool> IsAllowedAsync(string identifier, int maxRequests = 100, TimeSpan? timeWindow = null)
    {
        var window = timeWindow ?? TimeSpan.FromMinutes(1);
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            if (_requestCounts.TryGetValue(identifier, out var data))
            {
                // Reset if window has passed
                if (now - data.LastReset > window)
                {
                    _requestCounts[identifier] = (now, 1);
                    return Task.FromResult(true);
                }

                // Check if within limit
                if (data.RequestCount >= maxRequests)
                {
                    return Task.FromResult(false);
                }

                // Increment count
                _requestCounts[identifier] = (data.LastReset, data.RequestCount + 1);
                return Task.FromResult(true);
            }
            else
            {
                // First request
                _requestCounts[identifier] = (now, 1);
                return Task.FromResult(true);
            }
        }
    }
}
