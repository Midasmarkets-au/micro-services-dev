using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Middleware;
using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Agent Area filter, check if the agent account is valid.
/// </summary>
public class AgentAreaFilter(AccountManageService accManSvc) : AreaFilter
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        var isSuperAdmin = context?.HttpContext.User.IsSuperAdmin();
        if (isSuperAdmin == true)
        {
            await next();
            return;
        }

        var uid = GetRouteValue(context, RouteValueTypes.AgentUid);
        if (!await IsValidAgent(context, uid))
        {
            if (context != null) context.Result = new NotFoundObjectResult(Result.Error(ResultMessage.Common.AgentUidNotFound));
            return;
        }

        await next();
    }

    private async Task<bool> IsValidAgent(ActionContext? context, long uid)
    {
        if (uid < 1) return false;
        
        var partyId = context?.HttpContext.User.GetPartyId() ?? -1;
        if (partyId == -1) return false;

        var agentId = await accManSvc.GetAccountIdByUidAsync(uid);
        return await accManSvc.IsAccountBelongToPartyAsync(partyId, agentId);
    }
}