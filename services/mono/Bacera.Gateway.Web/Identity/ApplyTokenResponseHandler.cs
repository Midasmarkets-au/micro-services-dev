using Microsoft.AspNetCore;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// 拦截 ApplyTokenResponse 阶段：
///   1. 若 access_token 存在，将其写入 HttpOnly cookie。
///   2. 若存在 custom_response（多租户/2FA 特殊情况），写出自定义 JSON 并终止管道。
///      正常登录由 OpenIddict 自动返回标准 token JSON。
/// </summary>
public class ApplyTokenResponseHandler : IOpenIddictServerHandler<ApplyTokenResponseContext>
{
    // Must run before ProcessJsonResponse (order 500_000) so headers are not yet sent,
    // but after token generation is complete (which happens in HandleTokenRequest).
    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
        = OpenIddictServerHandlerDescriptor
            .CreateBuilder<ApplyTokenResponseContext>()
            .UseSingletonHandler<ApplyTokenResponseHandler>()
            .SetOrder(499_000)
            .Build();

    public async ValueTask HandleAsync(ApplyTokenResponseContext context)
    {
        var httpRequest = context.Transaction.GetHttpRequest();
        if (httpRequest is null)
            return;

        var httpResponse = httpRequest.HttpContext.Response;

        // Set access_token cookie whenever a token is issued.
        // Use Secure+SameSite=None for HTTPS (production/staging).
        // In local dev (HTTP), also use SameSite=None to support cross-origin requests.
        var accessToken = context.Response.AccessToken;
        if (!string.IsNullOrEmpty(accessToken))
        {
            var isHttps   = httpRequest.HttpContext.Request.IsHttps;
            var expiresIn = (int)(context.Response.ExpiresIn ?? 86400);
            AppendAccessTokenCookie(httpResponse, accessToken, expiresIn, isHttps);

            context.Response.AccessToken  = null;
            context.Response.RefreshToken = null;
            context.Response.TokenType    = null;
            context.Response.ExpiresIn    = null;
            context.Response.Scope        = null;
        }

        // Custom responses (multi-tenant selector, 2FA prompt) are now written directly
        // in PasswordGrantHandler during the HandleTokenRequest phase, so nothing to do here.
    }

    /// <summary>
    /// Shared helper for writing the access_token HttpOnly cookie.
    /// Used by both the OpenIddict token endpoint (this handler) and
    /// programmatic token issuance paths (e.g. email-code login in BcrTokenService).
    /// </summary>
    public static void AppendAccessTokenCookie(HttpResponse response, string accessToken, int expiresInSeconds, bool isHttps)
    {
        // In local dev (HTTP), allow cross-origin cookie by using SameSite=None with Secure=false.
        // Browsers (Chrome 80+) normally require Secure for SameSite=None, but grant an exception
        // for localhost, so this works in local testing environments.
        var isDev    = AppEnvironment.IsDevelopment();
        var useNone  = isHttps || isDev;
        response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            // SameSite=None requires Secure=true in all browsers (including Safari on localhost).
            // In dev we set Secure=true even over HTTP so the cookie is accepted cross-origin.
            Secure   = useNone || isHttps,
            SameSite = useNone ? SameSiteMode.None : SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds),
            Path     = "/",
        });
    }

    /// <summary>
    /// Shared helper for deleting the access_token cookie (logout).
    /// </summary>
    public static void DeleteAccessTokenCookie(HttpResponse response, bool isHttps)
    {
        var isDev   = AppEnvironment.IsDevelopment();
        var useNone = isHttps || isDev;
        response.Cookies.Delete("access_token", new CookieOptions
        {
            HttpOnly = true,
            Secure   = useNone || isHttps,
            SameSite = useNone ? SameSiteMode.None : SameSiteMode.Lax,
            Path     = "/",
        });
    }
}
