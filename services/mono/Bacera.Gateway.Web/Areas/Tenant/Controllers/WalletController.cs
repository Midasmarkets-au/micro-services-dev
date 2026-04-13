
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bacera.Gateway.Services.Common;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Wallet;
using MSG = ResultMessage.Wallet;
using MSGCommon = ResultMessage.Common;

[Tags("Tenant/Wallet")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class WalletController(AccountingService accountingService, TenantDbContext tenantDbContext) : TenantBaseController
{
    /// <summary>
    /// Wallet pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<M, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var hideEmail = ShouldHideEmail();
        var result = await accountingService.WalletQueryAsync(criteria, hideEmail);
        return Ok(result);
    }

    [HttpGet("snapshot")]
    public async Task<IActionResult> SnapshotIndex([FromQuery] WalletDailySnapshot.Criteria? criteria)
    {
        criteria ??= new WalletDailySnapshot.Criteria();
        var hideEmail = ShouldHideEmail();
        var items = await tenantDbContext.WalletDailySnapshots
            .PagedFilterBy(criteria)
            .ToTenantPageModel(hideEmail)
            .ToListAsync();

        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Wallet Statistic
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("statistic")]
    [ProducesResponseType(typeof(List<WalletStatisticViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Statistic([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var result = await accountingService.WalletStatisticAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Wallet
    /// </summary>
    /// <param name="walletId"></param>
    /// <returns></returns>
    [HttpGet("{walletId:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long walletId)
    {
        var result = await accountingService.WalletGetAsync(walletId);
        if (result.IsEmpty())
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Wallet Transactions pagination
    /// </summary>
    /// <param name="walletId"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{walletId:long}/transaction")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transactions(long walletId, [FromQuery] WalletTransaction.Criteria? criteria)
    {
        criteria ??= new WalletTransaction.Criteria();
        criteria.WalletId = walletId;
        var result = await accountingService.WalletTransactionQueryAsync(criteria);
        return Ok(result);
    }

    [HttpPut("{walletId:long}/fund-type")]
    public async Task<IActionResult> ChangeFundType(long walletId, [FromQuery] FundTypes fundType)
    {
        var wallet = await tenantDbContext.Wallets.SingleOrDefaultAsync(x => x.Id == walletId);
        if (wallet == null)
            return NotFound();
        wallet.FundType = (int)fundType;
        tenantDbContext.Wallets.Update(wallet);
        await tenantDbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("adjust")]
    public async Task<IActionResult> AdjustIndex([FromQuery] WalletAdjust.Criteria? criteria)
    {
        criteria ??= new WalletAdjust.Criteria();
        var items = await tenantDbContext.WalletAdjusts
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        return Ok(Result<List<WalletAdjust.TenantPageModel>, WalletAdjust.Criteria>.Of(items, criteria));
    }

    [HttpPost("{walletId:long}/adjust")]
    public async Task<IActionResult> CreateAdjust(long walletId, [FromBody] WalletAdjust.ManualAdjustCreateSpec spec)
    {
        // Validate spec.Amount != 0
        if (spec.Amount == 0)
        {
            return BadRequest(ToErrorResult(MSGCommon.InvalidAmount));
        }

        var operatorPartyId = GetPartyId();
        var wallet = await tenantDbContext.Wallets
            .Where(x => x.Id == walletId)
            .Select(x => new { x.PartyId, x.FundType, x.CurrencyId, x.IsPrimary })
            .SingleOrDefaultAsync();

        if (wallet == null) return NotFound("__WALLET_NOT_FOUND__");
        if (wallet.IsPrimary == 0) return BadRequest(ToErrorResult(MSG.OnlyPrimaryWalletAllowed));

        var entity = spec.ToEntity(walletId);
        entity.Amount = spec.Amount.ToScaledFromCents();
        entity.IdNavigation.AddActivity(StateTypes.Initialed,StateTypes.WalletAdjustCompleted, operatorPartyId);
        tenantDbContext.WalletAdjusts.Add(entity);
        await tenantDbContext.SaveChangesAsync();

        await accountingService.WalletChangeBalanceAsync(wallet.PartyId, (FundTypes)wallet.FundType, entity.Id, entity.Amount,
            (CurrencyTypes)wallet.CurrencyId, GetPartyId());

        return Ok();
    }
}