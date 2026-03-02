using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway;

public sealed partial class AccountingService
{
    public async Task<Result<List<RebateViewModel>, Rebate.Criteria>> RebateQueryAsync(
        Rebate.Criteria criteria, bool hideEmail = false)
    {
        var items = await _tenantDbContext.Rebates
            .PagedFilterBy(criteria)
            .ToTenantViewModel(hideEmail)
            .ToListAsync();
        criteria.PageTotalAmount = items.Sum(x => x.Amount);
        criteria.PageTotalVolume = items.Sum(x => x.Trade?.Volume ?? 0);
        return Result<List<RebateViewModel>, Rebate.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Rebate.ClientResponseModel>, Rebate.Criteria>> RebateQueryForClientAsync(
        Rebate.Criteria criteria)
    {
        var items = await _tenantDbContext.Rebates
            .PagedFilterBy(criteria)
            .ToClientResponseModel()
            .ToListAsync();
        criteria.PageTotalAmount = items.Sum(x => x.Amount);
        criteria.PageTotalVolume = items
            .Where(x => x.Trade != null)
            .GroupBy(x => x.Trade!.Id)
            .Select(x => x.First().Trade!.Volume)
            .Sum();
        return Result<List<Rebate.ClientResponseModel>, Rebate.Criteria>.Of(items, criteria);
    }

    public async Task<Rebate> RebateGetAsync(long id)
    {
        return await _tenantDbContext.Rebates.FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? new Rebate();
    }

    public async Task<Rebate.ClientResponseModel> RebateGetForPartyAsync(long id, long partyId)
    {
        var item = await _tenantDbContext.Rebates
            .Where(x => x.PartyId.Equals(partyId))
            .ToClientResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? new Rebate.ClientResponseModel();
        return item;
    }

    public async Task<Rebate.ClientResponseModel> RebateGetForRepAsync(long id, long repUid)
    {
        var item = await _tenantDbContext.Rebates
            .Where(x => x.Account.ReferPath.StartsWith("." + repUid))
            .ToClientResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? new Rebate.ClientResponseModel();
        return item;
    }

    public async Task<Rebate.ClientResponseModel> RebateGetForAgentAsync(long id, long agentUid)
    {
        var item = await _tenantDbContext.Rebates
            .Where(x => x.Account.ReferPath.Contains(agentUid.ToString()))
            .ToClientResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? new Rebate.ClientResponseModel();
        return item;
    }

    public async Task<Rebate.ClientResponseModel> RebateGetForSalesAsync(long id, long salesUid)
    {
        var item = await _tenantDbContext.Rebates
            .Where(x => x.Account.ReferPath.Contains(salesUid.ToString()))
            .ToClientResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(id)) ?? new Rebate.ClientResponseModel();
        return item;
    }

    public async Task<Rebate> RebateCreateAsync(long partyId, long accountId, long rowId, long amount,
        CurrencyTypes currency, long operatorPartyId, int onHoldDays = 30)
    {
        var rebate = Rebate.Build(partyId, accountId, rowId, amount, currency, onHoldDays);
        rebate.IdNavigation.AddActivity(operatorPartyId, ActionTypes.RebateCreate);

        await _tenantDbContext.Rebates.AddAsync(rebate);
        await _tenantDbContext.SaveChangesAsync();
        return rebate;
    }


    // public async Task<bool> RebateCompleteAsync(Rebate rebate, long operatorPartyId)
    // {
    //     // get account fund type for wallet fund type
    //     var account = await _tenantDbContext.Accounts.SingleAsync(x => x.Id.Equals(rebate.AccountId));
    //     if (account.IsClosed != 0 || account.Status != (int)AccountStatusTypes.Activate)
    //     {
    //         _logger.LogWarning("RebateAccountNotActive. {AccountId}", rebate.AccountId);
    //         return false;
    //     }
    //
    //     var hasReleased = await _tenantDbContext.WalletTransactions
    //         .Where(x => x.MatterId == rebate.Id)
    //         .Where(x => x.Matter.Rebate != null && x.Matter.Rebate!.Amount == rebate.Amount)
    //         .AnyAsync();
    //     if (hasReleased)
    //     {
    //         _logger.LogWarning("RebateAlreadyReleased. {RebateId}", rebate.Id);
    //         return true;
    //     }
    //
    //     try
    //     {
    //         await WalletChangeBalanceAsync(rebate.PartyId, (FundTypes)account.FundType, rebate.Id, rebate.Amount,
    //             (CurrencyTypes)rebate.CurrencyId, operatorPartyId);
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError(e, "RebateToWalletFailed. {Message}", e.Message);
    //         return false;
    //     }
    //
    //     var result = await TransitAsync(rebate, ActionTypes.RebateComplete, operatorPartyId);
    //     if (result.Item1) return true;
    //
    //     _logger.LogError("RebateTransitFailed. {Message}", result.Item2);
    //     return false;
    // }
}