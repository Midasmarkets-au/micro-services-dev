
using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("api/v1/admin/security")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class SecurityController : AdminBaseController
{
    private readonly LoginSecurityService _loginSecurityService;
    private readonly RateLimiterService _rateLimiterService;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(
        LoginSecurityService loginSecurityService,
        RateLimiterService rateLimiterService,
        ILogger<SecurityController> logger)
    {
        _loginSecurityService = loginSecurityService;
        _rateLimiterService = rateLimiterService;
        _logger = logger;
    }

    /// <summary>
    /// Check lockout status for a user
    /// </summary>
    /// <param name="identifier">Email or username</param>
    [HttpGet("lockout-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLockoutStatus([FromQuery] string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return BadRequest(new { message = "Identifier is required" });
        }

        var (isLocked, remaining, failedAttempts) = 
            await _loginSecurityService.CheckLockoutStatusAsync(identifier);

        return Ok(new
        {
            identifier,
            isLocked,
            remainingMinutes = remaining?.TotalMinutes ?? 0,
            failedAttempts,
            checkedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Unlock a user account (admin action)
    /// </summary>
    /// <param name="request">Unlock request with identifier (email) and reason</param>
    [HttpPost("unlock-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlockAccount([FromBody] UnlockAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
        {
            return BadRequest(new { message = "Identifier is required" });
        }

        var adminEmail = GetUserEmail();
        var success = await _loginSecurityService.UnlockAccountAsync(request.Identifier, adminEmail);

        if (success)
        {
            _logger.LogWarning(
                "Account unlocked by admin. Identifier: {Identifier}, Admin: {Admin}, Reason: {Reason}",
                request.Identifier, adminEmail, request.Reason);

            return Ok(new
            {
                message = "Account unlocked successfully",
                identifier = request.Identifier,
                unlockedBy = adminEmail,
                unlockedAt = DateTime.UtcNow
            });
        }

        return BadRequest(new { message = "Failed to unlock account" });
    }

    /// <summary>
    /// Reset rate limit for IP or email
    /// </summary>
    [HttpPost("reset-rate-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetRateLimit([FromBody] ResetRateLimitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
        {
            return BadRequest(new { message = "Key is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest(new { message = "Type is required (login_ip, login_email, etc.)" });
        }

        await _rateLimiterService.ResetRateLimitAsync(request.Key, request.Type);

        var adminEmail = GetUserEmail();
        _logger.LogWarning(
            "Rate limit reset by admin. Key: {Key}, Type: {Type}, Admin: {Admin}",
            request.Key, request.Type, adminEmail);

        return Ok(new
        {
            message = "Rate limit reset successfully",
            key = request.Key,
            type = request.Type,
            resetBy = adminEmail,
            resetAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current rate limit status
    /// </summary>
    [HttpGet("rate-limit-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRateLimitStatus([FromQuery] string key, [FromQuery] string type)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Key is required" });
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            return BadRequest(new { message = "Type is required (login_ip, login_email, etc.)" });
        }

        var currentCount = await _rateLimiterService.GetCurrentCountAsync(key, type);

        return Ok(new
        {
            key,
            type,
            currentCount,
            checkedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get list of currently locked accounts
    /// </summary>
    [HttpGet("locked-accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLockedAccounts()
    {
        var lockedAccounts = await _loginSecurityService.GetLockedAccountsAsync();

        return Ok(new
        {
            count = lockedAccounts.Count,
            accounts = lockedAccounts,
            retrievedAt = DateTime.UtcNow
        });
    }
}

public class UnlockAccountRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class ResetRateLimitRequest
{
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}


