
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Wallet;

[Tags("Client/Wallet")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class WalletController : ClientBaseController
{
    private readonly AccountingService _accountSvc;
    private readonly TenantDbContext _tenantDbContext;

    public WalletController(AccountingService accountingService, TenantDbContext tenantDbContext)
    {
        _accountSvc = accountingService;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Wallet Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), 200)]
    public async Task<IActionResult> Index(
        [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var result = await _accountSvc.WalletQueryForClientAsync(GetPartyId(), criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Wallet by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await _accountSvc.WalletGetForPartyAsync(id, GetPartyId());
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Wallet Transaction Pagination
    /// </summary>
    /// <param name="id"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/transaction")]
    [ProducesResponseType(typeof(Result<List<WalletTransaction.ResponseModel>, WalletTransaction.Criteria>), 200)]
    public async Task<IActionResult> Transaction(long id, [FromQuery] WalletTransaction.Criteria? criteria)
    {
        criteria ??= new WalletTransaction.Criteria();
        criteria.WalletId = id;
        criteria.PartyId = GetPartyId();
        var result = await _accountSvc.WalletTransactionQueryAsync(GetPartyId(), criteria);
        return Ok(result);
    }

    [HttpGet("{id:long}/withdrawal")]
    public async Task<IActionResult> Withdrawal(long id, [FromQuery] Withdrawal.Criteria? criteria)
    {
        criteria ??= new Withdrawal.Criteria();
        criteria.PartyId = GetPartyId();
        criteria.SourceType = TransactionAccountTypes.Wallet;
        var wallet = await _tenantDbContext.Wallets
            .Where(x => x.Id == id)
            .Select(x => new { x.CurrencyId, x.FundType })
            .SingleOrDefaultAsync();
        if (wallet == null) return NotFound();

        criteria.CurrencyId = (CurrencyTypes)wallet.CurrencyId;
        criteria.FundType = (FundTypes)wallet.FundType;
        var query = _tenantDbContext.Withdrawals
            .FilterBy(criteria)
            .ToClientWalletPageModel();
        var items = await query.OrderByDescending(x => x.CreatedOn)
            .Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size)
            .ToListAsync();
        criteria.Total = await query.CountAsync();
        return Ok(Result<List<M.WalletTransactionViewModel>, Withdrawal.Criteria>.Of(items, criteria));
    }

    [HttpGet("{id:long}/transfer")]
    public async Task<IActionResult> Transfer(long id, [FromQuery] Transaction.Criteria? criteria)
    {
        criteria ??= new Transaction.Criteria();
        criteria.WalletId = id;
        var items = await _tenantDbContext.Transactions
            .PagedFilterBy(criteria)
            .ToClientWalletPageModel()
            .ToListAsync();

        var accountIds = items.Select(x => x.Source)
            .Union(items.Select(x => x.Target))
            .Where(x => x != 0)
            .Distinct().ToList();

        var accounts = await _tenantDbContext.Accounts
            .Where(x => accountIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.AccountNumber);

        foreach (var item in items)
        {
            if (item.Source != 0)
            {
                accounts.TryGetValue(item.Source, out var source);
                item.Source = source;
            }

            if (item.Target != 0)
            {
                accounts.TryGetValue(item.Target, out var target);
                item.Target = target;
            }
        }

        return Ok(Result<List<M.WalletTransactionViewModel>, Transaction.Criteria>.Of(items, criteria));
    }
}