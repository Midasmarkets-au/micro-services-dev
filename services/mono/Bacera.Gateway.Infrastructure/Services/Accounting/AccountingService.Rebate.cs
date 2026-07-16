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

    // RebateK8s has no EF navigation to TradeRebateK8s (partitioned, composite-keyed table),
    // so Symbol/AccountNumber/TicketNumber filters and the Trade field are resolved via
    // separate TradeRebateK8s lookups instead of a single joined query.
    private async Task ResolveTradeRebateFilterAsync(RebateK8s.Criteria criteria)
    {
        if (!criteria.NeedsTradeRebateLookup) return;
        var query = _tenantDbContext.TradeRebateK8s.AsQueryable();
        if (criteria.Symbol.IsTangible()) query = query.Where(t => t.Symbol == criteria.Symbol);
        if (criteria.AccountNumber.IsTangible()) query = query.Where(t => t.AccountNumber == criteria.AccountNumber);
        if (criteria.TicketNumber.IsTangible()) query = query.Where(t => t.Ticket == criteria.TicketNumber);
        criteria.MatchedTradeRebateIds = await query.Select(t => t.Id).ToListAsync();
    }

    private async Task<Dictionary<long, TradeRebateK8s>> FetchTradeRebatesAsync(IEnumerable<long?> tradeRebateIds)
    {
        var ids = tradeRebateIds.Where(x => x != null).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<long, TradeRebateK8s>();
        var trades = await _tenantDbContext.TradeRebateK8s
            .Include(t => t.Account!.Party)
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
        return trades.ToDictionary(t => t.Id);
    }

    public async Task<Result<List<RebateViewModel>, RebateK8s.Criteria>> RebateK8sQueryAsync(
        RebateK8s.Criteria criteria, bool hideEmail = false)
    {
        await ResolveTradeRebateFilterAsync(criteria);

        var raw = await _tenantDbContext.RebateK8s
            .PagedFilterBy(criteria)
            .Select(x => new
            {
                x.TradeRebateId,
                ViewModel = new RebateViewModel
                {
                    PartyId = x.PartyId,
                    CurrencyId = (CurrencyTypes)x.CurrencyId,
                    Amount = (long)Math.Round(x.Amount),
                    TargetAccount = new AccountBasicViewModel
                    {
                        Id = x.AccountId,
                        Uid = x.Account.Uid,
                        Code = x.Account.Code,
                        Group = x.Account.Group,
                        PartyId = x.Account.PartyId,
                        Role = (AccountRoleTypes)x.Account.Role,
                        User = x.Account.Party.ToTenantBasicViewModel(hideEmail),
                    },
                    PostedOn = x.Matter.PostedOn,
                    User = x.Party.ToTenantBasicViewModel(hideEmail),
                }
            })
            .ToListAsync();

        var trades = await FetchTradeRebatesAsync(raw.Select(x => x.TradeRebateId));

        foreach (var x in raw)
        {
            if (x.TradeRebateId != null && trades.TryGetValue(x.TradeRebateId.Value, out var trade))
            {
                x.ViewModel.Trade = new TradeRebate.SummaryResponseModel
                {
                    Volume = trade.Volume,
                    Ticket = trade.Ticket,
                    Symbol = trade.Symbol,
                };
            }
        }

        var items = raw.Select(x => x.ViewModel).ToList();
        criteria.PageTotalAmount = items.Sum(x => x.Amount);
        criteria.PageTotalVolume = raw
            .Where(x => x.TradeRebateId != null && trades.ContainsKey(x.TradeRebateId.Value))
            .GroupBy(x => x.TradeRebateId!.Value)
            .Select(x => trades[x.Key].Volume)
            .Sum();
        return Result<List<RebateViewModel>, RebateK8s.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Rebate.ClientResponseModel>, RebateK8s.Criteria>> RebateK8sQueryForClientAsync(
        RebateK8s.Criteria criteria)
    {
        await ResolveTradeRebateFilterAsync(criteria);

        var raw = await _tenantDbContext.RebateK8s
            .PagedFilterBy(criteria)
            .Select(x => new
            {
                x.TradeRebateId,
                Model = new Rebate.ClientResponseModel
                {
                    Id = x.Id,
                    TradeRebateId = x.TradeRebateId,
                    Amount = (long)Math.Round(x.Amount),
                    AccountUid = x.Account.Uid,
                    CurrencyId = x.CurrencyId,
                    HoldUntilOn = x.HoldUntilOn,
                    CreatedOn = x.Matter.PostedOn,
                    Information = x.Information,
                    StateId = x.Matter.StateId,
                }
            })
            .ToListAsync();

        var trades = await FetchTradeRebatesAsync(raw.Select(x => x.TradeRebateId));

        foreach (var x in raw)
        {
            x.Model.Trade = x.TradeRebateId != null && trades.TryGetValue(x.TradeRebateId.Value, out var trade)
                ? new TradeRebate.SummaryResponseModel
                {
                    Id = trade.Id,
                    Symbol = trade.Symbol,
                    AccountNumber = trade.AccountNumber,
                    Ticket = trade.Ticket,
                    Volume = trade.Volume,
                    AccountName = trade.Account?.Party?.NativeName ?? "",
                    AccountUid = trade.Account?.Uid ?? 0,
                    CloseAt = trade.ClosedOn,
                    CurrencyId = trade.CurrencyId,
                }
                : new TradeRebate.SummaryResponseModel();
        }

        var items = raw.Select(x => x.Model).ToList();
        criteria.PageTotalAmount = items.Sum(x => x.Amount);
        criteria.PageTotalVolume = raw
            .Where(x => x.TradeRebateId != null && trades.ContainsKey(x.TradeRebateId.Value))
            .GroupBy(x => x.TradeRebateId!.Value)
            .Select(x => trades[x.Key].Volume)
            .Sum();

        var totalsSource = _tenantDbContext.RebateK8s.FilterBy(criteria);
        var totalAmount = await totalsSource.SumAsync(x => (decimal?)x.Amount) ?? 0;
        criteria.TotalAmount = (long)Math.Round(totalAmount);
        var allTradeRebateIds = await totalsSource
            .Where(x => x.TradeRebateId != null)
            .Select(x => x.TradeRebateId!.Value)
            .Distinct()
            .ToListAsync();
        criteria.TotalVolume = allTradeRebateIds.Count == 0
            ? 0
            : await _tenantDbContext.TradeRebateK8s
                .Where(t => allTradeRebateIds.Contains(t.Id))
                .SumAsync(t => t.Volume);

        return Result<List<Rebate.ClientResponseModel>, RebateK8s.Criteria>.Of(items, criteria);
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