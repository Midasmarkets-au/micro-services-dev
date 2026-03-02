using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Account;
using MSG = Bacera.Gateway.ResultMessage;

[Tags("Sales/Stat")]
public class StatController : SalesBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly TenantDbContext _tenantDbContext;
    private readonly ConfigurationService _configurationService;

    private const string RoutePrefix = "/api/v1";


    public StatController(TradingService tradingService, TenantDbContext tenantDbContext,
        ConfigurationService configurationService
    )
    {
        _tradingSvc = tradingService;
        _tenantDbContext = tenantDbContext;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Account pagination for Sales
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="uid"></param>
    /// <param name="date"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long salesUid, long uid, string date, DateTime? from, DateTime? to)
    {
        (var fromTime, var toTime) = Utils.CalculateTradePeriod(date, from, to);
        var account = await _tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .FirstOrDefaultAsync();
        if (account == null)
        {
            return NotFound(MSG.Common.UidNotFound);
        }

        if (!account.IsUpper(salesUid))
        {
            return NotFound(MSG.Common.UidNotFound);
        }

        var statData = new Dictionary<string, object>();
        statData["From"] = fromTime;
        statData["To"] = toTime;
        statData["NewAccountCount"] = await _tradingSvc.GetNewAccountCountByUid(account, fromTime, toTime);
        statData["NewAgentCount"] = await _tradingSvc.GetNewAgentCountByUid(account, fromTime, toTime);
        statData["DepositAmount"] = await _tradingSvc.GetDepositAmountByUid(account, fromTime, toTime);
        statData["WithdrawAmount"] = await _tradingSvc.GetWithdrawAmountByUid(account, fromTime, toTime);
        statData["RebateAmount"] = await _tradingSvc.GetRebateAmountByUid(account, fromTime, toTime);
        statData["TradeSymbol"] = await _tradingSvc.GetTradeSymbolByUid(account, fromTime, toTime);
        return Ok(statData);
    }
}