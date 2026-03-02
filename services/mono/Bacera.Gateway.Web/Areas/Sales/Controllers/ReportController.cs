using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

[Tags("Sales/Report")]
public class ReportController : SalesBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly TenantDbContext _tenantDbContext;

    public ReportController(TradingService tradingService, TenantDbContext tenantDbContext)
    {
        _tradingSvc = tradingService;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Get last trade
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="count"></param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    [HttpGet("trade/latest")]
    [ProducesResponseType(typeof(List<TradeViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LastTrade(long salesUid, [FromQuery] int? count, [FromQuery] int? serviceId)
    {
        if (serviceId == null)
        {
            serviceId = await _tenantDbContext.TradeServices
                .Where(x =>
                    x.Platform == (int)PlatformTypes.MetaTrader4
                    || x.Platform == (int)PlatformTypes.MetaTrader5)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }

        var items = await _tradingSvc
            .GetLastTradeTransactionForSalesAsync(salesUid, serviceId.Value,
                Math.Min(count ?? 5, 10));
        return Ok(Result.Of(items));
    }

    /// <summary>
    /// Get last transfer
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    [HttpGet("transaction/latest")]
    [ProducesResponseType(typeof(List<Transaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LastTransfer(long salesUid, [FromQuery] int? count)
    {
        var items = await _tradingSvc
            .GetLastTransferForSalesAsync(salesUid, Math.Min(count ?? 5, 10));
        return Ok(Result.Of(items));
    }
}