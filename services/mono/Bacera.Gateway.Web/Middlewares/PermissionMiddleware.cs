using System.Net;
using System.Text.RegularExpressions;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.Permission;

namespace Bacera.Gateway.Web.Middleware;

public partial class PermissionMiddleware(
    UserService userSvc,
    PermissionService permissionSvc,
    MonitorService monitorSvc) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // await next(context);
        // return;
        var action = context.Request.Path.Value ?? "/none";
        // remove "?"
        var index = action.IndexOf('?');
        if (index > 0)
        {
            action = action[..index];
        }

        if (IsPathExempt(action))
        {
            await next(context);
            return;
        }

        // Public configuration — unauthenticated page-load request; tenant is resolved from Host header
        if (action == "/api/v1/tenant/configuration" &&
            context.Request.Query["category"] == "public")
        {
            await next(context);
            return;
        }

        var tenantId = context.User.GetTenantId();
        var partyId = context.User.GetPartyId();
        if (context.User.IsSuperAdmin() || context.User.IsTenantAdmin())
        {
            _ = monitorSvc.UpdateUserRealTimeAsync(tenantId, partyId);
            await next(context);
            return;
        }

        // if (context.User.IsNewTestAdmin())
        // {
        //     await next(context);
        //     return;
        // }
 
        if (tenantId > 0 && partyId > 0 && await userSvc.IsUserLockedOutAsync(partyId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        

        // Not authenticated at all → 401 so the frontend interceptor can trigger a token refresh/re-login
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        // Authenticated but does not hold any admin role → 403
        if (!context.User.IsAdmin())
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        var method = context.Request.Method;
        var isAllowed = await permissionSvc.IsActionAllowForUser(tenantId, partyId, action, method);
        if (isAllowed)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
        await context.Response.WriteAsync($"Forbidden Party:{Party.HashEncode(partyId)}");
    }

    private static bool IsPathExempt(string path)
        => ExemptPaths.Any(x => path == x || path.StartsWith(x)) || !TenantApiRegex().Match(path).Success;

    private static readonly string[] ExemptPaths =
    [
        "/api/v1/client", "/api/status/ping", "/hub/client", "/api/v1/tenant/message/send-popup",
        "/api/configuration", "/api/v1/user/me", "/connect/token", "/none",
        "/api/v2/tenant/statistic",
    ];

    [GeneratedRegex(@"^/api/v\d+/tenant/.*")]
    private static partial Regex TenantApiRegex();
}