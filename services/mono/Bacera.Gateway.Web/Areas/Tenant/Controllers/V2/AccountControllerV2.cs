using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[Area("Tenant")]
[Tags("Tenant/Account")]
[Route("api/" + VersionTypes.V2 + "/[Area]/account")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class AccountControllerV2(
    TenantDbContext tenantCtx,
    CentralDbContext centralCtx,
    ITradingApiService tradingApiSvc,
    MyDbContextPool pool,
    IMyCache cache,
    AccountManageService accManageSvc,
    ISendMessageService sendMsgSvc)
    : TenantBaseControllerV2
{
    /// <summary>
    /// Confirm an account that is auto-created by the system.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/confirm-auto-create")]
    public async Task<IActionResult> ConfirmAutoOpenAccount(long id)
    {
        var account = await tenantCtx.Accounts
            .Include(x => x.Tags.Where(y => y.Name == AccountTagTypes.AutoCreate))
            .SingleOrDefaultAsync(x => x.Id == id);
        if (account == null) return NotFound();

        var autoOpenTag = account.Tags.SingleOrDefault(x => x.Name == AccountTagTypes.AutoCreate);
        if (autoOpenTag == null) return BadRequest("Account is not auto open");

        await sendMsgSvc.SendEventToManagerAsync(GetTenantId(), new EventNotice());
        account.Tags.Remove(autoOpenTag);
        await tenantCtx.SaveChangesAsync();
        await accManageSvc.AddAccountTagsAsync(id, AccountTagTypes.AutoCreateConfirmed);
        return NoContent();
    }

    /// <summary>
    /// Assign a wallet to an account.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="walletId"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/assign-wallet/{walletId:long}")]
    public async Task<IActionResult> AssignWalletToAccount(long id, long walletId)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null) return NotFound();

        var wallet = await tenantCtx.Wallets
            .Select(x => new { x.Id, x.FundType, x.CurrencyId })
            .SingleOrDefaultAsync(x => x.Id == walletId);
        if (wallet == null) return NotFound();

        if (account.CurrencyId != wallet.CurrencyId || account.FundType != wallet.FundType)
            return BadRequest("Account currency or fund type does not match wallet");

        account.WalletId = walletId;
        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Get trade statistics for a single account.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/trade/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountTradeStat(long id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var accountNumbers = await tenantCtx.Accounts
            .Where(x => x.Id == id)
            .Select(x => x.AccountNumber)
            .ToListAsync();
        try
        {
            var result = await accManageSvc.GetTradeStatisticsByIdAsync(accountNumbers, from ?? DateTime.MinValue, to ?? DateTime.MaxValue);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Get trade statistics for multiple accounts.
    /// </summary>
    /// <param name="accountNumbers"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [HttpGet("trade/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTradeStat([FromQuery] List<long> accountNumbers, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var result = await accManageSvc.GetTradeStatisticsByIdAsync(accountNumbers, from ?? DateTime.MinValue, to ?? DateTime.MaxValue);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Get daily report for MetaTrader account.
    /// </summary>
    /// <param name="accountNumber"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    [HttpGet("daily-report/{accountNumber:long}")]
    public async Task<IActionResult> GetDailyReport(long accountNumber, [FromQuery] DateTime date)
    {
        var serviceId = await tenantCtx.Accounts
            .Where(x => x.AccountNumber == accountNumber)
            .Select(x => x.ServiceId)
            .SingleOrDefaultAsync();
        if (serviceId == 0) return NotFound();
        if (pool.GetPlatformByServiceId(serviceId) != PlatformTypes.MetaTrader5)
            return BadRequest("Not supported");

        date = DateTime.SpecifyKind(date, DateTimeKind.Utc).Date;

        var from = date
            .AddHours(Utils.IsCurrentDSTLosAngeles(date) ? 20 : 21)
            .AddMinutes(59)
            .AddSeconds(59);

        var res = await tradingApiSvc.GetDailyReport(serviceId, accountNumber, from, from.AddDays(1));
        return Ok(res);
    }

    /// <summary>
    /// Enable transfer permission for the account.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/enable-transfer")]
    public async Task<IActionResult> EnableTransfer(long id)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null) return NotFound();

        var permission = account.Permission.ToList();
        if (permission.Count == 0) return BadRequest("Account permission not set");

        permission[0] = '1';
        account.Permission = string.Join("", permission);

        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Disable transfer permission for the account.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/disable-transfer")]
    public async Task<IActionResult> DisableTransfer(long id)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null) return NotFound();

        var permission = account.Permission.ToList();
        if (permission.Count == 0) return BadRequest("Account permission not set");

        permission[0] = '0';
        account.Permission = string.Join("", permission);

        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// $netInOut = $deposit + $withdraw + $credit + $adjust;
    /// $pnl = $equity - $previousEquity - $netInOut;
    /// </summary>
    /// <returns></returns>
    [HttpGet("stat-top")]
    public async Task<IActionResult> TopAccountStat([FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] long? accountNumber, [FromQuery] int count)
    {
        var result = await tenantCtx.AccountStats
            .Where(x => from == null || x.Date >= from)
            .Where(x => to == null || x.Date <= to)
            .Where(x => accountNumber == null || x.Account.AccountNumber == accountNumber)
            .Select(x => new
            {
                x.Account.AccountNumber,
                NetInOut = x.DepositAmount + x.WithdrawAmount + x.Credit + x.Adjust,
                Pnl = x.Equity - x.PreviousEquity - (x.DepositAmount + x.WithdrawAmount + x.Credit + x.Adjust),
            })
            .GroupBy(x => x.AccountNumber)
            .Select(x => new
            {
                AccountNumber = x.Key,
                NetInOut = x.Sum(y => y.NetInOut),
                Pnl = x.Sum(y => y.Pnl),
            })
            .OrderByDescending(x => x.Pnl)
            .ThenByDescending(x => x.NetInOut)
            .Take(count)
            .ToListAsync();

        return Ok(result);
    }


    const string LoginToTenantIdHKey = "portal_api:LoginToTenantIdHkey";
    [AllowAnonymous]
    [HttpGet("trade-login-log")]
    public async Task<IActionResult> TradeAccountLoginLog([FromBody] Dictionary<string, string> body)
    {
        var login = long.Parse(body["login"]);
        var tidRaw = await cache.HGetStringAsync(LoginToTenantIdHKey, login.ToString());
        if (tidRaw == null)
        {
        }

        return Ok();
    }
}