using Bacera.Gateway.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway;

partial class TradingService
{
    public async Task<Result<List<TradeViewModel>, TradeViewModel.Criteria>> QueryTrade(
        TradeViewModel.Criteria criteria, bool includeClosedAccount = false)
    {
        if (criteria.ServiceId == null)
            return Result<List<TradeViewModel>, TradeViewModel.Criteria>.Of([], criteria);

        var accountInQuery = criteria.AccountNumber;
        var accountsInQuery = criteria.AccountNumbers;
        criteria = await ApplyAccountNumber(criteria, includeClosedAccount);
        var serviceId = criteria.ServiceId!.Value;
        if (serviceId != 10 && serviceId != 20 && serviceId != 30)
        {
            return Result<List<TradeViewModel>, TradeViewModel.Criteria>.Of([], criteria);
        }

        criteria.Platform = myDbContextPool.GetPlatformByServiceId(criteria.ServiceId!.Value);
        var items = criteria.Platform switch
        {
            PlatformTypes.MetaTrader5 => await QueryMt5(criteria),
            PlatformTypes.MetaTrader4 => await QueryMt4(criteria),
            _ => []
        };

        var accountNumbers = items.Select(x => x.AccountNumber).Distinct().ToList();
        var users = await dbContext.Accounts
            .Where(x => accountNumbers.Contains(x.AccountNumber))
            .Select(x => new { x.Party.Email, x.Party.NativeName, x.AccountNumber })
            .ToDictionaryAsync(x => x.AccountNumber, x => new { x.Email, x.NativeName });
        foreach (var item in items)
        {
            if (!users.TryGetValue(item.AccountNumber, out var user)) continue;
            item.Email = user.Email;
            item.NativeName = user.NativeName;
        }

        criteria.AccountNumber = accountInQuery;
        criteria.AccountNumbers = accountsInQuery;
        return Result<List<TradeViewModel>, TradeViewModel.Criteria>.Of(items, criteria);
    }

    /// <summary>
    /// Generate Trade report query, routing with mt4/mt5 (deal/position)
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    private async Task<IQueryable<TradeViewModel>> TradeReportQuery(
        TradeViewModel.ReportCriteria criteria)
    {
        var emptyQuery = new List<TradeViewModel>().AsQueryable();

        var trdSvc = await GetMetaTradeDatabaseOptions(criteria.ServiceId);
        if (trdSvc == null)
            return emptyQuery;

        criteria = await ApplyAccountNumber(criteria);

        criteria.Platform = (PlatformTypes)trdSvc.Platform;
        var query = criteria.Platform switch
        {
            PlatformTypes.MetaTrader5 => await QueryMt5TradeReport(criteria, trdSvc.Id),
            PlatformTypes.MetaTrader4 => await QueryMt4TradeReport(criteria, trdSvc.Id),
            _ => emptyQuery
        };
        return query;
    }

    public async Task<List<TradeViewModel>> QueryMt5(TradeViewModel.Criteria criteria)
    {
        if (criteria.ServiceId == null) return [];
        await using var ctx = myDbContextPool.CreateCentralMT5DbContextAsync(criteria.ServiceId.Value, true);

        if (criteria.IsClosed != true)
        {
            var positionCriteria = criteria.ToMt5PositionCriteria();
            var query = positionCriteria.PagedApplyTo(ctx.Mt5Positions);
            var positions = await query
                .ToTradeViewModel(criteria.ServiceId).ToListAsync();
            positionCriteria.MergeToViewModelCriteria(criteria, positions);
            return positions;
        }

        var dealsCriteria = criteria.ToMt5DealCriteria();
        var dealQuery = dealsCriteria.PagedApplyTo(ctx.Mt5Deals2025s);
        var deals = await dealQuery
            .ToTradeViewModel(_tenantId, criteria.ServiceId.Value)
            .ToListAsync();
        dealsCriteria.MergeToViewModelCriteria(criteria, deals);
        await FulfillOpenAtAndTicket(deals, ctx);
        return deals;
    }

    public async Task<List<TradeViewModel>> QueryMt4(TradeViewModel.Criteria criteria)
    {
        if (criteria.ServiceId == null) return [];

        await using var ctx = myDbContextPool.CreateCentralMT4DbContextAsync(criteria.ServiceId.Value, true);
        var mt4Criteria = criteria.ToMt4Criteria();
        mt4Criteria.SortField = null;
        var query = mt4Criteria.PagedApplyTo(ctx.Mt4Trades);
        var items = await query
            .OrderByDescending(x => x.OpenTime)
            .ToTradeViewModel(tenancy.GetTenantId(), criteria.ServiceId.Value)
            .ToListAsync();
        mt4Criteria.MergeToViewModelCriteria(criteria, items);
        return items;
    }

    private async Task<IQueryable<TradeViewModel>> QueryMt5TradeReport(TradeViewModel.ReportCriteria criteria,
        long serviceId)
    {
        var ctx = await GetMetaTrade5DbContext(serviceId);

        if (criteria.IsClosed != true)
        {
            var positionCriteria = criteria.ToMt5PositionCriteria();
            return positionCriteria
                .PagedApplyTo(ctx.Mt5Positions)
                .ToTradeViewModel();
        }

        var dealsCriteria = criteria.ToMt5DealCriteria();
        return dealsCriteria
            .PagedApplyTo(ctx.Mt5Deals2025s)
            .ToTradeViewModel();
    }

    private async Task<IQueryable<TradeViewModel>> QueryMt4TradeReport(TradeViewModel.ReportCriteria criteria,
        long serviceId)
    {
        var ctx = await GetMetaTrade4DbContext(serviceId);

        var mt4Criteria = criteria.ToMt4Criteria();
        return mt4Criteria
            .PagedApplyTo(ctx.Mt4Trades)
            .ToTradeViewModel();
    }

    private async Task<TradeViewModel.Criteria> ApplyAccountNumber(TradeViewModel.Criteria criteria,
        bool includeClosedAccount = false)
    {
        if (!criteria.HasAccountCriteria()) return criteria;

        var accountCriteria = criteria.ToAccountCriteria();
        if (includeClosedAccount == true)
        {
            accountCriteria.Statuses = [AccountStatusTypes.Activate, AccountStatusTypes.Pause];
            accountCriteria.IncludeClosed = true;
        }

        if (criteria.ParentAccountUid != null && criteria.Target != null)
        {
            if (long.TryParse(criteria.Target, out var accountNumber))
            {
                criteria.AccountNumber = accountNumber;
                // criteria.ParentAccountUid = null;
            }
            else
            {
                var uid = await dbContext.Accounts
                    .Where(x => x.Group.ToLower() == criteria.Target.ToLower() &&
                                x.Role != (int)AccountRoleTypes.Client)
                    .Where(x => x.ReferPath.Contains(criteria.ParentAccountUid!.Value.ToString()))
                    .Select(x => x.Uid)
                    .FirstOrDefaultAsync();
                if (uid == 0)
                {
                    uid = await dbContext.AccountExtraRelations
                        .Where(x => x.ParentAccount.Uid == criteria.ParentAccountUid)
                        .Where(x => x.ChildAccount.Group.ToLower() == criteria.Target.ToLower())
                        .Select(x => x.ChildAccount.Uid)
                        .FirstOrDefaultAsync();
                }

                if (uid != 0)
                {
                    criteria.ParentAccountUid = uid;
                }
                else
                {
                    criteria.ParentAccountUid = -1;
                    criteria.AccountNumbers = [];
                    return criteria;
                }
            }
        }

        if (criteria.ParentAccountUid != null)
        {
            accountCriteria.PathStartWith = await dbContext.Accounts
                .Where(x => x.Uid == criteria.ParentAccountUid)
                .Select(x => x.ReferPath)
                .SingleOrDefaultAsync();
        }

        var logins = await dbContext.Accounts
            .FilterBy(accountCriteria)
            .Select(x => x.AccountNumber)
            .Where(x => x > 0)
            .ToListAsync();
        criteria.AccountNumbers = logins;
        return criteria;
    }

    private async Task<TradeViewModel.ReportCriteria> ApplyAccountNumber(TradeViewModel.ReportCriteria criteria)
    {
        if (!criteria.HasAccountCriteria()) return criteria;

        var accountCriteria = criteria.ToAccountCriteria();
        if (criteria.ParentAccountUid != null)
        {
            accountCriteria.PathStartWith = await dbContext.Accounts
                .Where(x => x.Uid == criteria.ParentAccountUid)
                .Select(x => x.ReferPath)
                .SingleOrDefaultAsync();
        }

        var logins = await dbContext.Accounts
            .FilterBy(accountCriteria)
            .Where(x => x.TradeAccount != null && x.TradeAccount.ServiceId == criteria.ServiceId)
            .Where(x => x.TradeAccount != null)
            .Select(x => x.TradeAccount!.AccountNumber)
            .ToListAsync();
        criteria.AccountNumbers = logins;
        return criteria;
    }

    private static async Task<List<TradeViewModel>> FulfillOpenAtAndTicket(List<TradeViewModel> items,
        MetaTrade5DbContext ctx)
    {
        var list = items
            .Where(x => x.Position != null)
            .Select(x => x.Position)
            .ToList();

        var positions = await ctx.Mt5Deals2025s
            .Where(x => list.Contains((long)x.PositionId))
            .Where(x => x.VolumeClosed == 0)
            .Select(x => new { x.PositionId, x.TimeMsc, x.Price, x.Order })
            .ToListAsync();

        foreach (var item in items)
        {
            if (item.Position == null) continue;
            var pos = positions.FirstOrDefault(x => (long)x.PositionId == item.Position);
            if (pos == null) continue;

            item.OpenAt = pos.TimeMsc;
            item.OpenPrice = pos.Price;
            // item.Ticket = (long)pos.Order;
        }

        return items;
    }

    private readonly Dictionary<long, TradeService> _tradeServicePool = new();
    private readonly Dictionary<int, MetaTrade4DbContext> _mt4DbContextPool = new();
    private readonly Dictionary<int, MetaTrade5DbContext> _mt5DbContextPool = new();

    public async Task<List<TradeService>> GetTradeServicesAsync()
    {
        var tradeServices = await dbContext.TradeServices
            .Where(x => x.Platform == (short)PlatformTypes.MetaTrader4
                        || x.Platform == (short)PlatformTypes.MetaTrader5)
            .ToListAsync();
        foreach (var service in tradeServices)
        {
            _tradeServicePool.Add(service.Id, service);
        }

        return tradeServices;
    }

    public async Task<MetaTrade4DbContext> GetMetaTrade4DbContext(long serviceId, bool? enableLog = true)
    {
        TradeService? trdSvc;
        if (_tradeServicePool.TryGetValue(serviceId, out var value))
        {
            trdSvc = value;
        }
        else
        {
            trdSvc = await dbContext.TradeServices
                .Where(x => x.Id == serviceId)
                .FirstOrDefaultAsync();

            if (trdSvc == null) throw new Exception("TradeServiceOptions not found");
            _tradeServicePool.Add(serviceId, trdSvc);
        }

        if (_mt4DbContextPool.TryGetValue(trdSvc.Id, out var context))
            return context;

        var traderServiceOptions = trdSvc.GetOptions<TradeServiceOptions>();
        if (!traderServiceOptions.IsDatabaseValidated())
            throw new Exception($"TradeServiceOptions is not valid for MT4. ServiceId {trdSvc.Id}");

        var options = new DbContextOptionsBuilder<MetaTrade4DbContext>();
        options.UseMySql(traderServiceOptions.Database!.ConnectionString, ServerVersion.Parse("5.7.38-mysql"));
        if (enableLog == true)
        {
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine);
        }

        context = new MetaTrade4DbContext(options.Options);
        _mt4DbContextPool.Add(trdSvc.Id, context);
        return context;
    }

    public async Task<MetaTrade5DbContext> GetMetaTrade5DbContext(long serviceId, bool? enableLog = true)
    {
        TradeService? trdSvc = null;
        if (_tradeServicePool.TryGetValue(serviceId, out var value))
        {
            trdSvc = value;
        }
        else
        {
            trdSvc = await dbContext.TradeServices
                .Where(x => x.Id == serviceId)
                .FirstOrDefaultAsync();

            if (trdSvc == null) throw new Exception("TradeServiceOptions not found");
            _tradeServicePool.Add(serviceId, trdSvc);
        }

        if (_mt5DbContextPool.TryGetValue(trdSvc.Id, out var context))
            return context;

        var traderServiceOptions = trdSvc.GetOptions<TradeServiceOptions>();
        if (!traderServiceOptions.IsDatabaseValidated())
            throw new Exception($"TradeServiceOptions is not valid for MT5. ServiceId {trdSvc.Id}");

        var options = new DbContextOptionsBuilder<MetaTrade5DbContext>();
        options.UseMySql(traderServiceOptions.Database!.ConnectionString, ServerVersion.Parse("5.7.38-mysql"));
        if (enableLog == true)
        {
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine);
        }

        context = new MetaTrade5DbContext(options.Options);
        _mt5DbContextPool.Add(trdSvc.Id, context);
        return context;
    }

    public async Task<TradeService?> GetMetaTradeDatabaseOptions(long serviceId)
    {
        if (_tradeServicePool.TryGetValue(serviceId, out var trdSvc))
            return trdSvc;

        trdSvc = await dbContext.TradeServices
            .Where(x => x.Id == serviceId)
            .FirstOrDefaultAsync();
        if (trdSvc == null) return null;

        _tradeServicePool.Add(serviceId, trdSvc);
        return trdSvc;
    }
}