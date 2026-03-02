using System.Net;
using Bacera.Gateway.Web.Services; // Add this for BcrLog

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Additional origin validation middleware for production security.
/// This provides an extra layer of protection beyond CORS.
/// </summary>
public class OriginValidationMiddleware : IMiddleware
{
    private readonly bool _isProduction;
    private readonly string[] _allowedOrigins;

    public OriginValidationMiddleware()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
        var isStaging = environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);
        var isTesting = environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);
        _isProduction = isProduction; // TODO: Do not use in staging and testing now just in case cause trouble
        _allowedOrigins = GetAllowedOrigins();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip validation in non-production environments
        if (!_isProduction)
        {
            await next(context);
            return;
        }

        // Skip validation for certain paths (health checks, etc.)
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (IsExemptPath(path))
        {
            await next(context);
            return;
        }

        // Validate Origin header
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        if (!string.IsNullOrEmpty(origin) && !IsAllowedOrigin(origin))
        {
            await RejectRequest(context, $"Forbidden: Invalid origin '{origin}'");
            return;
        }

        // Validate Referer header
        var referer = context.Request.Headers["Referer"].FirstOrDefault();
        if (!string.IsNullOrEmpty(referer) && !IsAllowedReferer(referer))
        {
            await RejectRequest(context, $"Forbidden: Invalid referer '{referer}'");
            return;
        }

        // Log suspicious requests
        if (IsSuspiciousRequest(context))
        {
            var ip = GetClientIpAddress(context);
            BcrLog.Slack($"Suspicious request from IP: {ip}, Origin: {origin}, Referer: {referer}, Path: {path}");
        }

        await next(context);
    }

    private string[] GetAllowedOrigins()
    {
        var originsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "";
        
        if (!string.IsNullOrEmpty(originsEnv))
        {
            return originsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(o => o.Trim().ToLowerInvariant())
                           .Where(o => !string.IsNullOrEmpty(o))
                           .ToArray();
        }

        // Fallback to default production domains (should match Startup.cs)
        return new[]
        {
            "https://midasmarkets.net",
            "https://tenant.midasmkts.com",
            "https://midasmkts.com",
            "https://au.midasmkts.com",
            "https://bvi.midasmkts.com",
            "https://sea.midasmkts.com",
            "https://www.midasmkts.com"
        };
    }

    private bool IsAllowedOrigin(string origin)
    {
        if (string.IsNullOrEmpty(origin)) return false;
        
        var normalizedOrigin = origin.ToLowerInvariant();
        return _allowedOrigins.Any(allowed => 
            normalizedOrigin == allowed || 
            normalizedOrigin.StartsWith(allowed.Replace("https://", "https://*.")) // Support subdomains
        );
    }

    private bool IsAllowedReferer(string referer)
    {
        if (string.IsNullOrEmpty(referer)) return true; // Allow empty referer
        
        try
        {
            var uri = new Uri(referer);
            var origin = $"{uri.Scheme}://{uri.Host}".ToLowerInvariant();
            return IsAllowedOrigin(origin);
        }
        catch
        {
            return false; // Invalid URI format
        }
    }

    private static bool IsExemptPath(string path)
    {
        var exemptPaths = new[]
        {
            "/health",
            "/status",
            "/api/v1/auth/ip-info",
            "/api/v2/domain",
            "/api/v2/welcome",
            "/connect/token",
            "/hf_manage"  // Hangfire has its own BasicAuth protection
        };

        return exemptPaths.Any(exempt => path.StartsWith(exempt));
    }

    private static bool IsSuspiciousRequest(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        var referer = context.Request.Headers["Referer"].FirstOrDefault();
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

        // Check for common attack patterns
        return string.IsNullOrEmpty(userAgent) ||
               userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
               (string.IsNullOrEmpty(origin) && string.IsNullOrEmpty(referer) && 
                context.Request.Method != "GET") ||
               (!string.IsNullOrEmpty(origin) && origin.Contains("localhost")) ||
               (!string.IsNullOrEmpty(referer) && referer.Contains("localhost"));
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            var ips = forwardedHeader.Split(',');
            if (ips.Length > 0 && IPAddress.TryParse(ips[0].Trim(), out _))
            {
                return ips[0].Trim();
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static async Task RejectRequest(HttpContext context, string message)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(message);
    }
} 