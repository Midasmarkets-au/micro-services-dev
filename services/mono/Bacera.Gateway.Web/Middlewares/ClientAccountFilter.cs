using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Agent Area filter, check if the agent account is valid.
/// </summary>
public class ClientAccountFilter(AccountManageService accManSvc) : AreaFilter
{

    public override async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        var id = await accManSvc.GetAccountIdByUidAsync(GetRouteValue(context, RouteValueTypes.AccountUid));
        if (context == null) return;

        var user = context.HttpContext.User;
        if (user.IsSuperAdmin())
        {
            await next();
            return;
        }

        var partyId = user.GetPartyId();
        var isValid = await accManSvc.IsAccountBelongToPartyAsync(partyId, id);
        if (!isValid)
        {
            context.Result = new NotFoundObjectResult(Result.Error("Account not found"));
            return;
        }

        await next();
    }
}