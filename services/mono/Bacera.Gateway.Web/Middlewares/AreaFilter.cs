using Microsoft.AspNetCore.Mvc.Filters;

namespace Bacera.Gateway.Web.Middlewares;

public abstract class AreaFilter : IAsyncActionFilter
{
    public virtual async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        await next();
    }

    protected static long GetRouteValue(ActionExecutingContext? context, string routeValueName)
        => ParseId(context?.RouteData.Values[routeValueName]?.ToString() ?? "-1");

    protected static string? GetStringRouteValue(ActionExecutingContext? context, string routeValueName)
        => context?.RouteData.Values[routeValueName]?.ToString();

    private static long ParseId(string idString)
        => string.IsNullOrEmpty(idString) ? -1 : long.TryParse(idString, out var id) ? id : -1;
}