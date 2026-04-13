
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Account Check")]
[Area("Tenant")]
[Route("api/" + VersionTypes.V1 + "/[Area]/account-check")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class AccountCheckController(
    ITenantGetter tGetter,
    TenantDbContext tenantCtx,
    IMyCache cache,
    MyDbContextPool pool,
    ConfigService cfgSvc,
    IBackgroundJobClient client)
    : TenantBaseController
{
    private readonly long _tenantId = tGetter.GetTenantId();
    private readonly string _hKey = CacheKeys.GetAccountCheckHashKey();

    /// <summary>
    /// Account Check List
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await tenantCtx.AccountChecks
            .OrderByDescending(x => x.CreatedOn)
            .ToTenantPageModel()
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Create Account Check 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccountCheck.CreateAndUpdateSpec spec)
    {
        var entity = spec.ToEntity();
        foreach (var accountNumber in spec.AccountNumbers)
        {
            var cacheModel = new AccountCheck.CacheModel
            {
                TenantId = _tenantId,
                Name = spec.Name,
                Type = spec.Type,
                Status = spec.Status
            };
            await cache.HSetStringAsync(_hKey, accountNumber.ToString(), Utils.JsonSerializeObject(cacheModel));
        }

        entity.OperatorPartyId = GetPartyId();
        tenantCtx.AccountChecks.Add(entity);
        await tenantCtx.SaveChangesAsync();
        return Ok(entity);
    }

    /// <summary>
    /// Update Account Check
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] AccountCheck.CreateAndUpdateSpec spec)
    {
        var entity = await tenantCtx.AccountChecks.FindAsync(id);
        if (entity == null) return NotFound();

        foreach (var accountNumber in entity.GetAccountNumbers())
        {
            await cache.HSetDeleteByKeyFieldAsync(_hKey, accountNumber.ToString());
        }

        spec.ApplyToEntity(entity);
        if (entity.Status == (int)AccountCheckStatusTypes.Active)
        {
            foreach (var accountNumber in entity.GetAccountNumbers())
            {
                var cacheModel = new AccountCheck.CacheModel
                {
                    TenantId = _tenantId,
                    Name = spec.Name,
                    Type = spec.Type,
                    Status = spec.Status
                };
                await cache.HSetStringAsync(_hKey, accountNumber.ToString(), Utils.JsonSerializeObject(cacheModel));
            }
        }

        entity.OperatorPartyId = GetPartyId();
        entity.UpdatedOn = DateTime.UtcNow;
        await tenantCtx.SaveChangesAsync();
        return Ok(entity);
    }

    /// <summary>
    ///  Delete Account Check
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await tenantCtx.AccountChecks.FindAsync(id);
        if (entity == null) return NotFound();

        foreach (var accountNumber in entity.GetAccountNumbers())
        {
            await cache.HSetDeleteByKeyFieldAsync(_hKey, accountNumber.ToString());
        }

        tenantCtx.AccountChecks.Remove(entity);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Reset Cache
    /// </summary>
    /// <returns></returns>
    [HttpPut("reset-cache")]
    public async Task<IActionResult> ResetCache()
    {
        await cache.HSetDeleteByKeyAsync(_hKey);
        var items = await tenantCtx.AccountChecks
            .Where(x => x.Status == (int)AccountCheckStatusTypes.Active)
            .ToListAsync();

        foreach (var item in items)
        {
            foreach (var accountNumber in item.GetAccountNumbers())
            {
                var cacheModel = new AccountCheck.CacheModel
                {
                    TenantId = _tenantId,
                    Name = item.Name,
                    Type = (AccountCheckTypes)item.Type,
                    Status = (AccountCheckStatusTypes)item.Status
                };
                await cache.HSetStringAsync(_hKey, accountNumber.ToString(), Utils.JsonSerializeObject(cacheModel));
            }
        }

        return Ok();
    }

    /// <summary>
    /// EquityBelowCredit
    /// </summary>
    /// <param name="page"></param>
    /// <param name="accountNumber"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpGet("equity-below-credit")]
    public async Task<IActionResult> EquityBelowCredit([FromQuery] int? page = null, [FromQuery] long? accountNumber = null,
        [FromQuery] string? email = null)
    {
        var excludedAccounts = await cfgSvc.GetAsync<List<long>>(nameof(Public), 0, ConfigKeys.ExcludedFromEquityBelowCredit);
        var accountNumbers = accountNumber == null ? null : new List<long> { accountNumber.Value };
        var result = await GetEquityBelowCreditAccountFromMTAsync(accountNumbers, excludedAccounts);

        page ??= 1;
        accountNumbers = result.Select(x => x.Key).ToList();
        var query = tenantCtx.Accounts.Where(x => accountNumbers.Contains(x.AccountNumber));
        if (!string.IsNullOrWhiteSpace(email)) query = query.Where(x => x.Party.Email.Contains(email));

        var items = await query
            .Skip((page.Value - 1) * 20)
            .Take(20)
            .OrderByDescending(x => x.Id)
            .ToEquityBelowCreditModel(pool.GetServiceNameDict())
            .ToListAsync();

        foreach (var item in items)
        {
            if (!result.TryGetValue(item.AccountNumber, out var value))
                continue;
            item.Equity = value.Equity;
            item.Credit = value.Credit;
            item.Balance = value.Balance;
            item.MtGroup = value.MtGroup;
            item.Margin = value.Margin;
        }

        var total = await query.CountAsync();
        var criteria = new { page, total, size = 20 };
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Send EquityBelowCredit Email
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("equity-below-credit/email")]
    public IActionResult SendEquityBelowCreditEmail([FromBody] AccountCheck.SendEmailSpec spec)
    {
        client.Enqueue<IGeneralJob>(x => x.EquityCheckEmailAsync(
            _tenantId,
            spec.AccountNumber,
            spec.Email,
            spec.Language,
            spec.BccEmails,
            spec.Date)
        );
        return Ok(Result.Success("Email sent successfully"));
    }


    private async Task<Dictionary<long, Account.EquityBelowCreditModel>> GetEquityBelowCreditAccountFromMTAsync(List<long>? included = null,
        List<long>? excludedAccounts = null)
    {
        var serviceIds = await tenantCtx.TradeServices
            // .Where(x => x.Platform == (int)PlatformTypes.MetaTrader4 || x.Platform == (int)PlatformTypes.MetaTrader5)
            .Where(x => x.Platform == (int)PlatformTypes.MetaTrader5)
            .Select(x => x.Id)
            .ToListAsync();

        var loginGroup = await Task.WhenAll(serviceIds.Select(async serviceId =>
        {
            if (!pool.IsServiceExisted(serviceId)) return new Dictionary<long, Account.EquityBelowCreditModel>();

            await using var tenantDbCtx = pool.CreateTenantDbContext(_tenantId);
            var platform = pool.GetPlatformByServiceId(serviceId);

            if (platform == PlatformTypes.MetaTrader4)
            {
                await using var ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
                var logins = await ctx.Mt4Users
                    .Where(x => x.Login > 0)
                    .Where(x => included == null || included.Contains(x.Login))
                    .Where(x => excludedAccounts == null || !excludedAccounts.Contains(x.Login))
                    .Where(x => x.Equity < x.Credit)
                    .Where(x => x.Equity > 0 || x.Credit != 0)
                    .Select(x => (long)x.Login)
                    .ToListAsync();

                var inCrm = await tenantDbCtx.Accounts
                    .Where(x => logins.Contains(x.AccountNumber))
                    .Select(x => x.AccountNumber)
                    .ToListAsync();

                var result = await ctx.Mt4Users
                    .Where(x => inCrm.Contains(x.Login))
                    .ToEquityBelowCreditModel(pool.GetServiceNameDict())
                    .ToListAsync();

                return result.ToDictionary(x => x.AccountNumber, x => x);
            }

            if (platform == PlatformTypes.MetaTrader5)
            {
                await using var ctx = pool.CreateCentralMT5DbContextAsync(serviceId);
                var logins = await ctx.Mt5Accounts
                    .Where(x => included == null || included.Contains((long)x.Login))
                    .Where(x => excludedAccounts == null || !excludedAccounts.Contains((long)x.Login))
                    .Where(x => x.Login > 0)
                    .Where(x => x.Equity < x.Credit)
                    .Where(x => x.Equity > 0 || x.Credit != 0)
                    .Select(x => (long)x.Login)
                    .ToListAsync();

                var inCrm = await tenantDbCtx.Accounts
                    .Where(x => logins.Contains(x.AccountNumber))
                    .Select(x => x.AccountNumber)
                    .ToListAsync();

                var result = await ctx.Mt5Accounts
                    .Where(x => inCrm.Contains((long)x.Login))
                    .ToEquityBelowCreditModel(pool.GetServiceNameDict())
                    .ToListAsync();

                var groupDict = await ctx.Mt5Users
                    .Where(x => logins.Contains((long)x.Login))
                    .ToDictionaryAsync(x => (long)x.Login, x => x.Group);

                result.ForEach(x => x.MtGroup = groupDict.GetValueOrDefault(x.AccountNumber, ""));
                return result.ToDictionary(x => x.AccountNumber, x => x);
            }

            return new Dictionary<long, Account.EquityBelowCreditModel>();
        }));

        return loginGroup.SelectMany(x => x).ToDictionary(x => x.Key, x => x.Value);
    }
}