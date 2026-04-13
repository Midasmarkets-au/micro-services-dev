
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Web.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.Account;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Account")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class AccountController(
    TradingService tradingService,
    // TenantDbContext tenantCtx,
    // IMyCache myCache,
    AccountManageService accManSvc,
    IGeneralJob generalJob,
    ITenantGetter tenantGetter)
    : ClientBaseController
{

    /// <summary>
    /// Account Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), 200)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>, M.Criteria>>> Index(
        [FromQuery] M.Criteria? criteria)
    {
        var updateBalanceTask = accManSvc.ConcurrentUpdateBalanceByPartyId(GetPartyId());
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        criteria.Size = 100;
        var items = await tradingService.AccountQueryForClientAsync(criteria, GetPartyId());
        await updateBalanceTask;
        return Ok(items);
    }

    /// <summary>
    /// Get Account by uid
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> Get(long uid)
    {
        var item = await tradingService.AccountClientResponseModelGetForPartyAsync(uid, GetPartyId());
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Get account wizard status
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/wizard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWizard(long uid)
    {
        var exists = await tradingService.AccountUidExistsForPartyAsync(uid, GetPartyId());
        if (exists.Item1 == false)
            return NotFound();

        var result = await tradingService.GetAccountWizardAsync(exists.Item2);
        return Ok(result);
    }

    /// <summary>
    /// Refresh Trade Account Status
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/refresh")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefreshTradeAccountStatus(long uid)
    {
        var accountId = await tradingService.TradeAccountLookupByUidAsync(uid);

        await Task.WhenAll(
            generalJob.TryUpdateTradeAccountStatus(tenantGetter.GetTenantId(), accountId, true),
            accManSvc.UpdateAccountSearchText(accountId)
        );

        var item = await tradingService.AccountGetAsync(accountId);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    // private async Task UpdateBalanceByPartyId()
    // {
    //     var partyId = GetPartyId();
    //     var cacheKey = $"account-balance-update-T:{GetTenantId()}-P:{partyId}";
    //     var cacheValue = await myCache.GetStringAsync(cacheKey);
    //     if (!string.IsNullOrEmpty(cacheValue))
    //         return;
    //
    //     var ids = await tenantCtx.TradeAccounts
    //         .Where(x => x.IdNavigation.PartyId == partyId)
    //         .Select(x => x.Id)
    //         .ToListAsync();
    //
    //     if (ids.Count == 0)
    //         return;
    //
    //     try
    //     {
    //         await Task.WhenAll(ids.Select(x => generalJob.TryUpdateTradeAccountStatus(GetTenantId(), x)));
    //     }
    //     catch
    //     {
    //         // ignored
    //     }
    //
    //
    //     await myCache.SetStringAsync(cacheKey, string.Join(',', ids), TimeSpan.FromSeconds(30));
    // }
}
