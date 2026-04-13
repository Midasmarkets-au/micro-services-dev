using Microsoft.AspNetCore;
using Microsoft.Net.Http.Headers;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Validation.OpenIddictValidationEvents;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// Extracts the access_token from the HttpOnly cookie and injects it as the Authorization
/// header so all downstream OpenIddict handlers see it as a normal Bearer token.
/// Runs BEFORE ExtractAccessTokenFromAuthorizationHeader (order 2000) so the cookie
/// value is already in the header when OpenIddict reads it.
/// The frontend no longer sends Bearer tokens — authentication is cookie-only.
/// </summary>
public class ExtractCookieTokenHandler : IOpenIddictValidationHandler<ProcessAuthenticationContext>
{
    public static OpenIddictValidationHandlerDescriptor Descriptor { get; }
        = OpenIddictValidationHandlerDescriptor
            .CreateBuilder<ProcessAuthenticationContext>()
            .UseSingletonHandler<ExtractCookieTokenHandler>()
            // Run BEFORE Authorization header extraction (order 2000).
            .SetOrder(OpenIddictValidationAspNetCoreHandlers.ExtractAccessTokenFromAuthorizationHeader.Descriptor.Order - 500)
            .Build();

    private static readonly ILogger _log =
        LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ExtractCookieTokenHandler>();

    public ValueTask HandleAsync(ProcessAuthenticationContext context)
    {
        var httpRequest = context.Transaction.GetHttpRequest();
        if (httpRequest is null)
        {
            _log.LogWarning("ExtractCookieTokenHandler: no HttpRequest in transaction");
            return default;
        }

        var token = httpRequest.Cookies["access_token"];
        _log.LogInformation("ExtractCookieTokenHandler: cookie={CookiePreview} path={Path}",
            string.IsNullOrEmpty(token) ? "<none>" : token[..Math.Min(20, token.Length)] + "…",
            httpRequest.Path);

        if (!string.IsNullOrEmpty(token))
        {
            // Inject cookie token as Authorization header so OpenIddict's standard
            // ExtractAccessTokenFromAuthorizationHeader handler picks it up normally.
            httpRequest.HttpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {token}";
        }

        return default;
    }
}
