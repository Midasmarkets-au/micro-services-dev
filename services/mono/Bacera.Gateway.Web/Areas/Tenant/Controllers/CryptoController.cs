using OpenIddict.Validation.AspNetCore;
using Amazon.Runtime.Internal.Util;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Crypto")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class CryptoController(TenantDbContext tenantCtx, CryptoService cryptoSvc, IMyCache cache) : TenantBaseController
{
    /// <summary>
    /// Crypto page - returns only non-deleted crypto wallets by default
    /// </summary>
    /// <param name="includeDeleted">Include soft-deleted wallets (default: false)</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] bool includeDeleted = false)
    {
        var hideEmail = ShouldHideEmail();
        var query = tenantCtx.Cryptos.AsQueryable();
        
        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }
        
        var items = await query
            .ToTenantPageModel(hideEmail)
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Crypto.CreateSpec spec)
    {
        var exists = await tenantCtx.Cryptos.AnyAsync(x => x.Address == spec.Address && !x.IsDeleted);
        if (exists) return BadRequest("Address already exists");

        var nameExists = await tenantCtx.Cryptos.AnyAsync(x => x.Name == spec.Name && !x.IsDeleted);
        if (nameExists) return BadRequest("Name already exists");

        var partyId = GetPartyId();
        var entity = await cryptoSvc.CreateCryptoAsync(partyId, spec.Name, spec.Type, spec.Address);
        return Ok(entity);
    }

    /// <summary>
    /// Sync a crypto's transaction
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/sync")]
    public async Task<IActionResult> SyncTransaction(long id)
    {
        var item = await tenantCtx.Cryptos.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound();
        await cryptoSvc.TronProSyncTransactionAsync(item);
        return Ok();
    }
    

    /// <summary>
    /// Crypto transactions
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("transaction")]
    public async Task<IActionResult> Transaction([FromQuery] CryptoTransaction.Criteria? criteria = null)
    {
        criteria ??= new CryptoTransaction.Criteria();
        var hideEmail = ShouldHideEmail();
        var items = await tenantCtx.CryptoTransactions
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Change the status of a transaction
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/status/{status}")]
    public async Task<IActionResult> ChangeStatus(long id, CryptoStatusTypes status)
    {
        var item = await tenantCtx.Cryptos
            .Where(x => x.Id == id && !x.IsDeleted)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();

        if (item.Status == (short)CryptoStatusTypes.InUse)
            return BadRequest("Cannot change status of a transaction that is in use");
        
        item.Status = (short)status;
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Soft delete a crypto wallet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var (success, message) = await cryptoSvc.SoftDeleteCryptoAsync(id);
        
        if (!success)
            return BadRequest(ToErrorResult(message));
        
        return Ok(new { message });
    }

    /// <summary>
    /// Restore a soft-deleted crypto wallet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/restore")]
    public async Task<IActionResult> Restore(long id)
    {
        var (success, message) = await cryptoSvc.RestoreCryptoAsync(id);
        
        if (!success)
            return BadRequest(ToErrorResult(message));
        
        return Ok(new { message });
    }

    /// <summary>
    /// Get request logs for a crypto wallet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("request-logs/{id:long}")]
    public async Task<IActionResult> GetRequestLogs(long id)
    {
        // get from sorted
        var crypto = await tenantCtx.Cryptos
            .Where(x => !x.IsDeleted)
            .Select(x => new { x.Id, x.Address })
            .SingleOrDefaultAsync(x => x.Id == id);
        
        if (crypto == null) return NotFound();
        
        var hourKey = DateTime.UtcNow.ToString("yyyyMMddHH");
        var key = CacheKeys.CryptoWalletKey(crypto.Address);
        var requestKey = $"{key}_{hourKey}";
        // var utcNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        // var startTime = new DateTimeOffset(DateTime.UtcNow).AddMinutes(-30).ToUnixTimeSeconds();
        // var curTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        // await cache.AddToSortedSetAsync(requestKey, json, unixTime, TimeSpan.FromMinutes(55));
        var logs = await cache.GetFromSortedSetAsync(requestKey, 0, 20);
        return Ok(logs.Select(Utils.JsonDeserializeDynamic));
    }
}