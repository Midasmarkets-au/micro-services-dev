
﻿using Bacera.Gateway.Services.Acct;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = PaymentInfo;

[Area("Client")]
[Tags("Client/Payment Info")]
[Route("api/" + VersionTypes.V1 + "/[Area]/payment-info")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class PaymentInfoController(TenantDbContext tenantDbContext, AcctService acctSvc) : ClientBaseController
{
    /// <summary>
    /// Payment Info Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        var items = await tenantDbContext.PaymentInfos
            .PagedFilterBy(criteria)
            .ToClientResponseModels()
            .ToListAsync();
        return Ok(Result<List<M.ClientResponseModel>, M.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Payment Info
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tenantDbContext.PaymentInfos
            .Where(x => x.PartyId == GetPartyId() && x.Id == id)
            .ToClientResponseModels()
            .SingleOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create Payment Info 
    /// </summary>
    /// <param name="createAndUpdateSpec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] M.CreateAndUpdateSpec createAndUpdateSpec)
    {
        if (createAndUpdateSpec.PaymentPlatform == PaymentPlatformTypes.USDT)
        {
            string walletAddress = createAndUpdateSpec.Info["walletAddress"];
            var isExist = await acctSvc.IsUSDTWalletExistAsync(walletAddress);
            if (isExist) return BadRequest("WALLET_ADDRESS_EXIST");
        }

        var item = new M
        {
            PartyId = GetPartyId(),
            Name = createAndUpdateSpec.Name,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            PaymentPlatform = (int)createAndUpdateSpec.PaymentPlatform,
            Info = JsonConvert.SerializeObject(createAndUpdateSpec.Info),
        };
        await tenantDbContext.PaymentInfos.AddAsync(item);
        await tenantDbContext.SaveChangesAsync();

        return Ok(item.ToResponseModel());
    }

    /// <summary>
    /// Update Payment Info 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var item = await tenantDbContext.PaymentInfos
            .Where(x => x.PartyId == GetPartyId())
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();

        if (item == null)
            return NotFound();

        if (item.PaymentPlatform == (int)PaymentPlatformTypes.USDT)
        {
            try
            {
                string walletAddress = spec.Info["walletAddress"];
                var isExist = await acctSvc.IsUSDTWalletExistAsync(walletAddress);
                if (isExist) return BadRequest("Wallet address already exists");
            }
            catch
            {
                return BadRequest("Cannot update wallet address");
            }
        }

        item.Info = JsonConvert.SerializeObject(spec.Info);
        item.UpdatedOn = DateTime.UtcNow;
        item.Name = spec.Name;

        tenantDbContext.PaymentInfos.Update(item);
        await tenantDbContext.SaveChangesAsync();

        return Ok(item.ToResponseModel());
    }

    /// <summary>
    /// Delete Payment Info 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await tenantDbContext.PaymentInfos
            .Where(x => x.PartyId == GetPartyId())
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();

        if (item == null)
            return NotFound();

        tenantDbContext.PaymentInfos.Remove(item);
        await tenantDbContext.SaveChangesAsync();

        return NoContent();
    }
}