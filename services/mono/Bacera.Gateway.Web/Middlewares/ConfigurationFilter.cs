using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Agent Area filter, check if the agent account is valid.
/// </summary>
public class ConfigurationFilter : AreaFilter
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        var category = GetStringRouteValue(context, RouteValueTypes.Category);
        if (context == null) return;

        if (category == null || !ConfigCategoryTypes.IsValid(category))
        {
            context.Result = new NotFoundObjectResult(Result.Error("Invalid category"));
            return;
        }

        await next();
    }
}