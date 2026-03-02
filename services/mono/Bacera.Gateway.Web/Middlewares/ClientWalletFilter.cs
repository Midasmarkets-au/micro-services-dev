using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bacera.Gateway.Web.Middlewares;

/// <summary>
/// Agent Area filter, check if the agent account is valid.
/// </summary>
public class ClientWalletFilter(WalletService walletSvc) : AreaFilter
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
    {
        var hashId = GetStringRouteValue(context, RouteValueTypes.HashId);
        if (context == null || hashId == null) return;

        var id = Wallet.HashDecode(hashId);
        var partyId = context.HttpContext.User.GetPartyId();
        var isValid = await walletSvc.IsWalletBelongToPartyAsync(partyId, id);
        if (!isValid)
        {
            context.Result = new NotFoundObjectResult(Result.Error("Wallet not found"));
            return;
        }

        await next();
    }
}