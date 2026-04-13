
﻿using Bacera.Gateway.Connection;
using Bacera.Gateway.Services;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.TradeService;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Trade Rebate")]
[Route("api/" + VersionTypes.V1 + "/[Area]/trade-rebate")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class TradeRebateController(
    TenantDbContext tenantDbContext,
    TenantDbConnection tenantCon,
    RebateService rebateSvc)
    : TenantBaseController
{
    /// <summary>
    /// Trade Rebate pagination (trade history)
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TradeRebate>, TradeRebate.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] TradeRebate.Criteria? criteria)
    {
        criteria ??= new TradeRebate.Criteria();
        var items = await tenantDbContext.TradeRebates
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result<List<TradeRebate>, TradeRebate.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Trade Rebate Detail
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var hideEmail = ShouldHideEmail();
        var item = await tenantDbContext.TradeRebates
            .Where(x => x.Id == id)
            .ToTenantViewModel(hideEmail)
            .FirstOrDefaultAsync() ?? new TradeRebateViewModel();

        item.Rebates = await tenantDbContext.Rebates
            .Where(x => item.RebateIds.Contains(x.Id))
            .ToRebateBasicViewModel(hideEmail)
            .ToListAsync();

        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    [HttpGet("{id:long}/check-rebates")]
    public async Task<IActionResult> CheckRebates(long id)
        // => Ok(await _tradingSvc.TradeRebateCheckForTenantAsync(id));
        => Ok(await rebateSvc.TradeRebateCheckForTenantAsync(id));

    [HttpGet("check-trade-sync")]
    public async Task<IActionResult> CheckTradeSync([FromQuery] DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        var sqlTradeRebate = $"""
                              SELECT
                                DATE_PART('hour', "CreatedOn") AS hour,
                                COUNT(*) AS count
                              FROM
                                trd.trade_rebate_k8s
                              WHERE
                                DATE(created_on) = DATE '{dateStr}'
                              GROUP BY
                                hour
                              ORDER BY
                                hour;
                              """;
        var sqlRebate = $"""
                         SELECT
                            DATE_PART('hour', "HoldUntilOn") AS hour,
                            COUNT(*) AS count
                         FROM
                            trd."_Rebate"
                         WHERE
                            DATE("HoldUntilOn") = DATE '{dateStr}'
                         GROUP BY
                            hour
                         ORDER BY
                            hour;
                         """;


        var tradeData = await tenantCon.ToListAsync<(int hour, int count)>(sqlTradeRebate);
        var rebateData = await tenantCon.ToListAsync<(int hour, int count)>(sqlRebate);
        var tradeDict = tradeData.ToDictionary(x => x.hour, x => x.count);
        var rebateDict = rebateData.ToDictionary(x => x.hour, x => x.count);
        var result = Enumerable.Range(0, 24)
            .Select(x => new
            {
                hour = x,
                tradeCount = tradeDict.GetValueOrDefault(x),
                rebateCount = rebateDict.GetValueOrDefault(x)
            })
            .ToList();

        return Ok(result);
    }
    

    [HttpPost("{id:long}/reset")]
    public async Task<IActionResult> Reset(long id)
    {
        var item = await tenantDbContext.TradeRebates.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        item.Status = (short)TradeRebateStatusTypes.Created;
        item.UpdatedOn = DateTime.UtcNow;
        await tenantDbContext.SaveChangesAsync();
        return Ok();
    }
}