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
        // Use Secure+SameSite=None for HTTPS (production/staging),
        // and Secure=false+SameSite=Lax for HTTP (local dev).
        var accessToken = context.Response.AccessToken;
        if (!string.IsNullOrEmpty(accessToken))
        {
            var isHttps   = httpRequest.HttpContext.Request.IsHttps;
            var expiresIn = context.Response.ExpiresIn ?? 86400;
            httpResponse.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure   = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires  = DateTimeOffset.UtcNow.AddSeconds(expiresIn),
                Path     = "/",
            });
        }

        // Custom responses (multi-tenant selector, 2FA prompt) are now written directly
        // in PasswordGrantHandler during the HandleTokenRequest phase, so nothing to do here.
    }
}
