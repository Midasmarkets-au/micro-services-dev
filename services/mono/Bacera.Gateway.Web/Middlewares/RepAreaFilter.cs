using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Rep Area filter, check if the rep account is valid.
/// </summary>
public class RepAreaFilter(AccountManageService accManSvc) : AreaFilter
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        var isSuperAdmin = context?.HttpContext.User.IsSuperAdmin();
        if (isSuperAdmin == true)
        {
            await next();
            return;
        }

        var uid = GetRouteValue(context, RouteValueTypes.RepUid);
        if (!await IsValidRep(context, uid))
        {
            if (context != null) context.Result = new NotFoundObjectResult(Result.Error(ResultMessage.Common.RepUidNotFound));
            return;
        }

        await next();
    }

    private async Task<bool> IsValidRep(ActionContext? context, long uid)
    {
        if (uid < 1) return false;
        var partyId = context?.HttpContext.User.GetPartyId() ?? -1;
        if (partyId == -1) return false;

        var repId = await accManSvc.GetAccountIdByUidAsync(uid);
        return await accManSvc.IsAccountBelongToPartyAsync(partyId, repId);
    }
}