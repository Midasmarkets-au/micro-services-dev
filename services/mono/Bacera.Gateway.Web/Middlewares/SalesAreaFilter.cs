using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Sales Area filter, check if the sales account is valid.
/// </summary>
public class SalesAreaFilter(AccountManageService accManageSvc) : AreaFilter
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        var isSuperAdmin = context?.HttpContext.User.IsSuperAdmin();
        if (isSuperAdmin == true)
        {
            await next();
            return;
        }

        var uid = GetRouteValue(context, RouteValueTypes.SalesUid);
        if (!await IsUidValid(context, uid))
        {
            if (context != null) context.Result = new NotFoundObjectResult(Result.Error(ResultMessage.Common.SalesUidNotFound));
            return;
        }

        var childUid = GetRouteValue(context, RouteValueTypes.ChildAccountUid);
        if (childUid != -1 && !await accManageSvc.IsAccountBelongToParentAsync(uid, childUid))
        {
            if (context != null) context.Result = new NotFoundObjectResult(Result.Error(ResultMessage.Common.UidNotFound));
            return;
        }

        await next();
    }

    private async Task<bool> IsUidValid(ActionContext? context, long uid)
    {
        if (uid < 1) return false;
        var partyId = context?.HttpContext.User.GetPartyId() ?? -1;
        if (partyId == -1) return false;

        var accountId = await accManageSvc.GetAccountIdByUidAsync(uid);
        return await accManageSvc.IsAccountBelongToPartyAsync(partyId, accountId, true);
    }
}