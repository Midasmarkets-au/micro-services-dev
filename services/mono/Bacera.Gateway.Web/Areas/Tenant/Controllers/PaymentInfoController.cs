using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Services.Acct;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = PaymentInfo;

[Area("Tenant")]
[Tags("Tenant/Payment Info")]
[Route("api/" + VersionTypes.V1 + "/[Area]/payment-info")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class PaymentInfoController(TenantDbContext tenantCtx, AcctService acctSvc) : TenantBaseController
{
    /// <summary>
    /// Payment Info Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.TenantResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await tenantCtx.PaymentInfos
            .PagedFilterBy(criteria)
            .ToTenantResponseModels()
            .ToListAsync();
        return Ok(Result<List<M.TenantResponseModel>, M.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Payment Info
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M.TenantResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tenantCtx.PaymentInfos
            .Where(x => x.Id == id)
            .ToTenantResponseModels()
            .SingleOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }
    
    /// <summary>
    /// Edit Payment Info
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(M.TenantResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PaymentInfoUpdate(long id, [FromBody] M.UpdateSpec spec)
    {
        var item = await tenantCtx.PaymentInfos
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();
        
        if (item.PaymentPlatform == (int)PaymentPlatformTypes.USDT)
        {
            string walletAddress = spec.Info["walletAddress"];
            var isExist = await acctSvc.IsUSDTWalletExistAsync(walletAddress);
            if (isExist) return BadRequest("Wallet address already exists");
        }
        
        item.Info = JsonConvert.SerializeObject(spec.Info);
        tenantCtx.PaymentInfos.Update(item);
        await tenantCtx.SaveChangesAsync();
        return Ok(item);
    }
    
    /// <summary>
    /// Delete Payment Info
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(M.TenantResponseModel), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PaymentInfoDelete(long id)
    {
        var item = await tenantCtx.PaymentInfos
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (item == null) return NotFound();

        tenantCtx.PaymentInfos.Remove(item);
        await tenantCtx.SaveChangesAsync();
        return Ok();
    }
}