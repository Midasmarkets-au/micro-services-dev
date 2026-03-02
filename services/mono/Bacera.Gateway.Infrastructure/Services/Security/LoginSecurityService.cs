using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services;

    /// <summary>
    /// Login attempt tracker and account locker service
    /// Tracks failed login attempts and locks accounts after threshold is exceeded
    /// Uses Redis for fast, distributed locking with organized key structure
    /// </summary>
    public class LoginSecurityService
    {
        private readonly IMyCache _cache;
        private readonly ILogger<LoginSecurityService> _logger;

        // Configuration
        private const int MaxFailedAttemptsBeforeLockout = 5;
        private const int LockoutDurationMinutes = 30;
        private const int FailedAttemptWindowMinutes = 15;

        // Redis key prefixes for better organization
        private const string FailedAttemptsPrefix = "security:login:failed";
        private const string LockoutPrefix = "security:login:locked";
        private const string RateLimitPrefix = "security:ratelimit";

        public LoginSecurityService(IMyCache cache, ILogger<LoginSecurityService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

    /// <summary>
    /// Record a failed login attempt
    /// </summary>
    /// <param name="identifier">Email, username, or user ID</param>
    /// <param name="ip">IP address of the attempt</param>
    /// <returns>Number of failed attempts and whether account is now locked</returns>
    public async Task<(int failedAttempts, bool isLocked, TimeSpan? lockoutRemaining)> RecordFailedLoginAsync(
        string identifier, 
        string ip)
    {
        var key = GetFailedAttemptsKey(identifier);
        var lockKey = GetLockoutKey(identifier);

        try
        {
            // Check if already locked out
            var lockValue = await _cache.GetStringAsync(lockKey);
            if (!string.IsNullOrEmpty(lockValue))
            {
                var lockoutRemaining = await _cache.GetTtlAsync(lockKey);
                _logger.LogWarning(
                    "Login attempt for locked account. Identifier: {Identifier}, IP: {IP}, Remaining: {Remaining}",
                    identifier, ip, lockoutRemaining);
                return (MaxFailedAttemptsBeforeLockout, true, lockoutRemaining);
            }

            // Increment failed attempts
            var countStr = await _cache.GetStringAsync(key);
            var failedAttempts = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr) + 1;
            
            var window = TimeSpan.FromMinutes(FailedAttemptWindowMinutes);
            await _cache.SetStringAsync(key, failedAttempts.ToString(), window);

            _logger.LogWarning(
                "Failed login attempt recorded. Identifier: {Identifier}, IP: {IP}, Attempts: {Attempts}/{Max}",
                identifier, ip, failedAttempts, MaxFailedAttemptsBeforeLockout);

            // Check if should lock account
            if (failedAttempts >= MaxFailedAttemptsBeforeLockout)
            {
                var lockoutDuration = TimeSpan.FromMinutes(LockoutDurationMinutes);
                await _cache.SetStringAsync(lockKey, DateTime.UtcNow.ToString("O"), lockoutDuration);
                
                _logger.LogError(
                    "Account locked due to too many failed attempts. Identifier: {Identifier}, IP: {IP}, Duration: {Duration} minutes",
                    identifier, ip, LockoutDurationMinutes);

                // Send alert
                await SendSecurityAlertAsync(identifier, ip, failedAttempts);

                return (failedAttempts, true, lockoutDuration);
            }

            return (failedAttempts, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording failed login for {Identifier}", identifier);
            return (0, false, null);
        }
    }

    /// <summary>
    /// Check if account is locked out
    /// </summary>
    public async Task<(bool isLocked, TimeSpan? remaining, int failedAttempts)> CheckLockoutStatusAsync(string identifier)
    {
        try
        {
            var lockKey = GetLockoutKey(identifier);
            var lockValue = await _cache.GetStringAsync(lockKey);
            
            if (!string.IsNullOrEmpty(lockValue))
            {
                var remaining = await _cache.GetTtlAsync(lockKey);
                var failedAttempts = await GetFailedAttemptsCountAsync(identifier);
                return (true, remaining, failedAttempts);
            }

            var attempts = await GetFailedAttemptsCountAsync(identifier);
            return (false, null, attempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking lockout status for {Identifier}", identifier);
            return (false, null, 0);
        }
    }

    /// <summary>
    /// Reset failed attempts counter (called after successful login)
    /// </summary>
    public async Task ResetFailedAttemptsAsync(string identifier)
    {
        try
        {
            var key = GetFailedAttemptsKey(identifier);
            await _cache.KeyDeleteAsync(key);
            
            _logger.LogInformation("Failed login attempts reset for {Identifier}", identifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting failed attempts for {Identifier}", identifier);
        }
    }

    /// <summary>
    /// Manually unlock an account (admin action) - resets lockout AND rate limits
    /// </summary>
    public async Task<bool> UnlockAccountAsync(string identifier, string adminEmail)
    {
        try
        {
            // Reset account lockout
            var lockKey = GetLockoutKey(identifier);
            var attemptsKey = GetFailedAttemptsKey(identifier);

            await _cache.KeyDeleteAsync(lockKey);
            await _cache.KeyDeleteAsync(attemptsKey);

            // Also reset rate limits for this account (email-based) - COMMENTED OUT
            // var emailRateLimitKey = $"security:ratelimit:login_email:{identifier}";
            // await _cache.KeyDeleteAsync(emailRateLimitKey);

            _logger.LogWarning(
                "Account manually unlocked. Identifier: {Identifier}, Admin: {Admin}, ResetKeys: [{LockKey}, {AttemptsKey}]",
                identifier, adminEmail, lockKey, attemptsKey);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account for {Identifier}", identifier);
            return false;
        }
    }

    /// <summary>
    /// Get current failed attempts count
    /// </summary>
    public async Task<int> GetFailedAttemptsCountAsync(string identifier)
    {
        try
        {
            var key = GetFailedAttemptsKey(identifier);
            var countStr = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get list of currently locked accounts (for monitoring)
    /// </summary>
    public async Task<List<LockedAccountInfo>> GetLockedAccountsAsync()
    {
        // This would require scanning Redis keys or maintaining a separate index
        // For now, return empty list
        // In production, consider maintaining a separate set of locked accounts
        return new List<LockedAccountInfo>();
    }

    private string GetFailedAttemptsKey(string identifier) => $"{FailedAttemptsPrefix}:{identifier.ToLower()}";
    private string GetLockoutKey(string identifier) => $"{LockoutPrefix}:{identifier.ToLower()}";

    private async Task SendSecurityAlertAsync(string identifier, string ip, int attempts)
    {
        try
        {
            // Send to Slack or other notification system
            var message = $"🚨 Account Lockout Alert\n" +
                         $"Identifier: {identifier}\n" +
                         $"IP: {ip}\n" +
                         $"Failed Attempts: {attempts}\n" +
                         $"Lockout Duration: {LockoutDurationMinutes} minutes\n" +
                         $"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
            
            BcrLog.Slack(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security alert for {Identifier}", identifier);
        }
    }

    public class LockedAccountInfo
    {
        public string Identifier { get; set; } = string.Empty;
        public DateTime LockedAt { get; set; }
        public TimeSpan RemainingLockout { get; set; }
        public int FailedAttempts { get; set; }
    }
}


