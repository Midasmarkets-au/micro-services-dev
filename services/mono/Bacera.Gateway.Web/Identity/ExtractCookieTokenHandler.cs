using Microsoft.AspNetCore;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Validation.OpenIddictValidationEvents;

namespace Bacera.Gateway.Web.Identity;

/// <summary>
/// Extracts the access_token from the HttpOnly cookie set by ApplyTokenResponseHandler
/// and injects it into the OpenIddict validation pipeline.
/// Runs before ExtractAccessTokenFromAuthorizationHeader so that the Bearer header
/// still takes precedence when both are present.
/// </summary>
public class ExtractCookieTokenHandler : IOpenIddictValidationHandler<ProcessAuthenticationContext>
{
    public static OpenIddictValidationHandlerDescriptor Descriptor { get; }
        = OpenIddictValidationHandlerDescriptor
            .CreateBuilder<ProcessAuthenticationContext>()
            .UseSingletonHandler<ExtractCookieTokenHandler>()
            .SetOrder(OpenIddictValidationAspNetCoreHandlers.ExtractAccessTokenFromAuthorizationHeader.Descriptor.Order - 500)
            .Build();

    public ValueTask HandleAsync(ProcessAuthenticationContext context)
    {
        if (!string.IsNullOrEmpty(context.AccessToken))
            return default;

        var httpRequest = context.Transaction.GetHttpRequest();
        if (httpRequest is null)
            return default;

        var token = httpRequest.Cookies["access_token"];
        if (!string.IsNullOrEmpty(token))
            context.AccessToken = token;

        return default;
    }
}
