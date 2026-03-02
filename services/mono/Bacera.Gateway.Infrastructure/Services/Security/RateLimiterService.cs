using Bacera.Gateway.Services.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services;

    /// <summary>
    /// Rate limiter service for login attempts and API requests
    /// Uses Redis for distributed rate limiting across multiple servers
    /// </summary>
    public class RateLimiterService
    {
        private readonly IMyCache _cache;
        private readonly ILogger<RateLimiterService> _logger;

        // Redis key prefix for better organization
        private const string RateLimitPrefix = "security:ratelimit";

        public RateLimiterService(IMyCache cache, ILogger<RateLimiterService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

    /// <summary>
    /// Check if request should be rate limited
    /// </summary>
    /// <param name="key">Unique identifier (IP, email, userId, etc.)</param>
    /// <param name="maxAttempts">Maximum attempts allowed</param>
    /// <param name="window">Time window</param>
    /// <param name="type">Type of rate limit for logging</param>
    /// <returns>True if request is allowed, false if rate limited</returns>
    public async Task<(bool allowed, int remainingAttempts, TimeSpan? retryAfter)> CheckRateLimitAsync(
        string key,
        int maxAttempts,
        TimeSpan window,
        string type = "request")
    {
        var cacheKey = $"{RateLimitPrefix}:{type}:{key}";

        try
        {
            // Get current count
            var countStr = await _cache.GetStringAsync(cacheKey);
            var currentCount = string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);

            if (currentCount >= maxAttempts)
            {
                // Get TTL to know when limit resets
                var ttl = await _cache.GetTtlAsync(cacheKey);
                var retryAfter = ttl ?? window;

                _logger.LogWarning(
                    "Rate limit exceeded for {Type}: {Key}. Count: {Count}/{Max}. Retry after: {RetryAfter}",
                    type, key, currentCount, maxAttempts, retryAfter);

                return (false, 0, retryAfter);
            }

            // Increment counter
            currentCount++;
            await _cache.SetStringAsync(cacheKey, currentCount.ToString(), window);

            var remaining = maxAttempts - currentCount;
            _logger.LogDebug(
                "Rate limit check passed for {Type}: {Key}. Count: {Count}/{Max}. Remaining: {Remaining}",
                type, key, currentCount, maxAttempts, remaining);

            return (true, remaining, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for {Type}: {Key}", type, key);
            // On error, allow request (fail open)
            return (true, maxAttempts, null);
        }
    }

    /// <summary>
    /// Reset rate limit counter for a key
    /// </summary>
    public async Task ResetRateLimitAsync(string key, string type = "request")
    {
        var cacheKey = $"{RateLimitPrefix}:{type}:{key}";
        await _cache.KeyDeleteAsync(cacheKey);
        _logger.LogInformation("Rate limit reset for {Type}: {Key}", type, key);
    }

    /// <summary>
    /// Get current count for a key
    /// </summary>
    public async Task<int> GetCurrentCountAsync(string key, string type = "request")
    {
        var cacheKey = $"{RateLimitPrefix}:{type}:{key}";
        var countStr = await _cache.GetStringAsync(cacheKey);
        return string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);
    }
}


