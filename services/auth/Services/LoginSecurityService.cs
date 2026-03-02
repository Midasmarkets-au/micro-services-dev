using StackExchange.Redis;

namespace Bacera.Gateway.Auth.Services;

/// <summary>
/// Mirrors mono's LoginSecurityService — tracks failed login attempts and locks accounts.
/// Uses Redis for distributed state (same key structure as mono).
/// </summary>
public class LoginSecurityService(IConnectionMultiplexer redis, ILogger<LoginSecurityService> logger)
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;
    private const int FailedAttemptWindowMinutes = 15;

    private const string FailedAttemptsPrefix = "security:login:failed";
    private const string LockoutPrefix = "security:login:locked";

    public async Task<(bool isLocked, TimeSpan? remaining, int failedAttempts)> CheckLockoutStatusAsync(string identifier)
    {
        try
        {
            var db = redis.GetDatabase();
            var lockKey = GetLockoutKey(identifier);
            var lockValue = await db.StringGetAsync(lockKey);

            if (!string.IsNullOrEmpty(lockValue))
            {
                var ttl = await db.KeyTimeToLiveAsync(lockKey);
                var attempts = await GetFailedAttemptsCountAsync(db, identifier);
                return (true, ttl, attempts);
            }

            var count = await GetFailedAttemptsCountAsync(db, identifier);
            return (false, null, count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking lockout status for {Identifier}", identifier);
            return (false, null, 0);
        }
    }

    public async Task<(int failedAttempts, bool isLocked, TimeSpan? lockoutRemaining)> RecordFailedLoginAsync(
        string identifier, string ip)
    {
        try
        {
            var db = redis.GetDatabase();
            var lockKey = GetLockoutKey(identifier);
            var lockValue = await db.StringGetAsync(lockKey);

            if (!string.IsNullOrEmpty(lockValue))
            {
                var remaining = await db.KeyTimeToLiveAsync(lockKey);
                return (MaxFailedAttempts, true, remaining);
            }

            var attemptsKey = GetFailedAttemptsKey(identifier);
            var countStr = await db.StringGetAsync(attemptsKey);
            var failedAttempts = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr!) + 1;

            var window = TimeSpan.FromMinutes(FailedAttemptWindowMinutes);
            await db.StringSetAsync(attemptsKey, failedAttempts.ToString(), window);

            logger.LogWarning(
                "Failed login attempt. Identifier: {Identifier}, IP: {IP}, Attempts: {Attempts}/{Max}",
                identifier, ip, failedAttempts, MaxFailedAttempts);

            if (failedAttempts >= MaxFailedAttempts)
            {
                var lockoutDuration = TimeSpan.FromMinutes(LockoutMinutes);
                await db.StringSetAsync(lockKey, DateTime.UtcNow.ToString("O"), lockoutDuration);

                logger.LogError(
                    "Account locked. Identifier: {Identifier}, IP: {IP}, Duration: {Duration}min",
                    identifier, ip, LockoutMinutes);

                return (failedAttempts, true, lockoutDuration);
            }

            return (failedAttempts, false, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error recording failed login for {Identifier}", identifier);
            return (0, false, null);
        }
    }

    public async Task ResetFailedAttemptsAsync(string identifier)
    {
        try
        {
            var db = redis.GetDatabase();
            await db.KeyDeleteAsync(GetFailedAttemptsKey(identifier));
            logger.LogInformation("Failed login attempts reset for {Identifier}", identifier);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting failed attempts for {Identifier}", identifier);
        }
    }

    private static async Task<int> GetFailedAttemptsCountAsync(IDatabase db, string identifier)
    {
        var countStr = await db.StringGetAsync(GetFailedAttemptsKey(identifier));
        return string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr!);
    }

    private static string GetFailedAttemptsKey(string identifier) => $"{FailedAttemptsPrefix}:{identifier.ToLower()}";
    private static string GetLockoutKey(string identifier) => $"{LockoutPrefix}:{identifier.ToLower()}";
}
