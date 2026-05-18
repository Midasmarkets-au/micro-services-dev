using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Integration;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    // Note: DepositPaymentCompleted (310) means payment received but NOT yet in user's wallet, so we exclude it
    private static readonly int[] DepositCompletedStates = 
    [
        (int)StateTypes.DepositCompleted, 
        (int)StateTypes.DepositCallbackCompleted
    ];
    
    private static readonly int WithdrawalCompletedState = (int)StateTypes.WithdrawalCompleted;
    
    private static readonly int RebateCompletedState = (int)StateTypes.RebateCompleted;

    // Trade source = MT5 deals (Mt5Deals2025s), not Postgres TradeRebate.
    // Aligned with Daily Equity Report and the Sales Center trade page.
    // Filter: Entry IN {Out=1, InOut=2 reverse, OutBy=3 hedge close} AND Action IN {Buy=0, Sell=1}
    private static readonly uint[] Mt5ClosingEntries = [1, 2, 3];
    private static readonly uint[] Mt5BuySellActions = [0, 1];

    /// <summary>
    /// Aggregated trade snapshot fetched from MT5 (Mt5Deals2025s) for the requested period.
    /// Replaces the old _TradeRebate-based aggregation (which counts only rebate-eligible
    /// trades and misses HasNoRebate / non-completed rebate rows).
    /// </summary>
    private sealed class Mt5TradeAggregates
    {
        public Dictionary<long, int> CountByAccountId { get; init; } = new();
        public Dictionary<long, double> VolumeByAccountId { get; init; } = new();
        public Dictionary<long, List<string>> SymbolsByAccountId { get; init; } = new();
        public Dictionary<DateTime, int> CountByDate { get; init; } = new();
        public Dictionary<DateTime, double> VolumeByDate { get; init; } = new();
        public Dictionary<string, int> CountBySymbol { get; init; } = new();
        public int TotalCount { get; set; }
        public double TotalVolume { get; set; }
    }

    /// <summary>
    /// Get sales statistics including hierarchy data, time series, summary stats, and product distribution
    /// </summary>
    public async Task<SalesStatistics.ResponseModel> GetSalesStatisticsAsync(SalesStatistics.Criteria criteria)
    {
        // 1. Verify sales account exists
        var salesAccount = await tenantDbContext.Accounts
            .Where(x => x.Uid == criteria.SalesUid)
            .Where(x => x.IsClosed == 0) // Only active accounts
            .FirstOrDefaultAsync();

        if (salesAccount == null)
        {
            throw new KeyNotFoundException("SALES_ACCOUNT_NOT_FOUND");
        }

        // 1.1 Handle from and to
        // By default only retriving last 30 days
        // Ensure From and To are in UTC - all times should be start/end of day at 00:00:00 UTC
        var todayUtc = DateTime.UtcNow.Date;
        
        // fromUtc should be start of day (00:00:00) in UTC
        var fromUtc = criteria.From.HasValue
            ? DateTime.SpecifyKind(criteria.From.Value.Date, DateTimeKind.Utc) // Start of specified day at 00:00:00 UTC
            : DateTime.SpecifyKind(todayUtc.AddDays(-30), DateTimeKind.Utc);   // Start of day 30 days ago at 00:00:00 UTC
      
        // toUtc should be end of the specified day (23:59:59.9999999) in UTC
        var toUtc = criteria.To.HasValue 
            ? DateTime.SpecifyKind(criteria.To.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1) // End of specified day
            : DateTime.SpecifyKind(todayUtc, DateTimeKind.Utc).AddDays(1).AddTicks(-1);   // End of today

        // Validate date range - prevent queries that are too large
        const int maxDaysRange = 90; // Maximum 90 days
        var daysDifference = (toUtc - fromUtc).TotalDays;
        if (daysDifference > maxDaysRange)
        {
            throw new ArgumentException($"DATE_RANGE_TOO_LARGE: Maximum allowed range is {maxDaysRange} days. Requested: {daysDifference:F0} days.");
        }

        // Validate date range order
        if (fromUtc > toUtc)
        {
            throw new ArgumentException("INVALID_DATE_RANGE: 'From' date must be earlier than 'To' date.");
        }

        // 2. Build the sales ReferPath pattern for querying descendants
        // Match ReferPath format: ".{parent}.{salesUid}.{child}" or ".{parent}.{salesUid}"
        // Using EF.Functions.Like with proper pattern matching (matches SQL: ilike '%%748125381.%')
        var salesReferPathPattern = $"%{salesAccount.Uid}.%";

        // 3. Get all descendant accounts (clients under this sales) + the sales account itself
        var descendantAccounts = await tenantDbContext.Accounts
            .Where(x => x.Uid == criteria.SalesUid || EF.Functions.Like(x.ReferPath, salesReferPathPattern))
            .Where(x => x.IsClosed == 0) // Only active accounts
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.PartyId,
                x.Name,
                x.Role,
                x.Group,
                x.ReferPath,
                x.Level,
                x.WalletId
            })
            .ToListAsync();

        // Get role names mapping
        var descendantRoleIds = descendantAccounts.Select(x => (long)x.Role).Distinct().ToList();
        var descendantRoleNames = await authDbContext.Roles
            .Where(x => descendantRoleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name ?? "Unknown");

        // 4. Get deposits for all descendants
        // Only include completed deposits: DepositCompleted (350) or DepositCallbackCompleted (345)
        // Group by TargetAccountId (not PartyId) so each account gets its own deposit value
        var descendantAccountIds = descendantAccounts.Select(x => x.Id).ToList();
        var deposits = await tenantDbContext.Deposits
            .Where(d => descendantAccountIds.Contains(d.TargetAccountId!.Value))
            .Where(d => d.IdNavigation.Type == (int)MatterTypes.Deposit) // Filter by MatterType for safety
            .Where(d => d.IdNavigation.StatedOn >= fromUtc && d.IdNavigation.StatedOn <= toUtc)
            .Where(d => d.CurrencyId == 840 || d.CurrencyId == 841) // USD or USC
            .Where(d => DepositCompletedStates.Contains(d.IdNavigation.StateId)) // Only completed deposits (money in wallet)
            .GroupBy(d => d.TargetAccountId!.Value) // Group by AccountId, not PartyId
            .Select(g => new
            {
                AccountId = g.Key,
                TotalAmount = g.Sum(d => d.CurrencyId == 840 ? d.Amount : d.Amount * 100) // Convert USC to USD cents
            })
            .ToListAsync();

        // 5. Get withdrawals for all descendants (SourceAccountId)
        // Only include completed withdrawals: WithdrawalCompleted (450)
        var withdrawalsByAccount = await tenantDbContext.Withdrawals
            .Where(w => w.SourceAccountId.HasValue && descendantAccountIds.Contains(w.SourceAccountId.Value))
            .Where(w => w.IdNavigation.Type == (int)MatterTypes.Withdrawal) // Filter by MatterType for safety
            .Where(w => w.IdNavigation.StatedOn >= fromUtc && w.IdNavigation.StatedOn <= toUtc)
            .Where(w => w.CurrencyId == 840 || w.CurrencyId == 841)
            .Where(w => w.IdNavigation.StateId == WithdrawalCompletedState) // Only completed withdrawals (final state)
            .GroupBy(w => w.SourceAccountId!.Value)
            .Select(g => new { AccountId = g.Key, TotalAmount = g.Sum(w => w.CurrencyId == 840 ? w.Amount : w.Amount * 100) })
            .ToListAsync();

        // 5b. Get wallet withdrawals for all descendants
        // Join withdrawals with accounts where PartyId matches AND SourceWalletId matches WalletId
        // Only include completed withdrawals: WithdrawalCompleted (450)
        var descendantPartyIds = descendantAccounts.Select(x => x.PartyId).Distinct().ToList();
        var withdrawalsByWallet = await (from w in tenantDbContext.Withdrawals
            join a in tenantDbContext.Accounts on w.PartyId equals a.PartyId
            where w.SourceWalletId.HasValue
               && w.SourceWalletId == a.WalletId
               && w.IdNavigation.Type == (int)MatterTypes.Withdrawal // Filter by MatterType for safety
               && w.IdNavigation.StatedOn >= fromUtc && w.IdNavigation.StatedOn <= toUtc
               && (w.CurrencyId == 840 || w.CurrencyId == 841)
               && w.IdNavigation.StateId == WithdrawalCompletedState // Only completed withdrawals (final state)
               && descendantAccountIds.Contains(a.Id) // Only accounts that are descendants
            group w by a.Id into g
            select new { AccountId = g.Key, TotalAmount = g.Sum(w => w.CurrencyId == 840 ? w.Amount : w.Amount * 100) })
            .ToListAsync();

        // 6. Get rebates for all descendants
        // Only include completed rebates: RebateCompleted (550)
        // Query Rebates table directly to access StateId via IdNavigation (similar to deposits/withdrawals)
        var rebates = await tenantDbContext.Rebates
            .Where(r => descendantAccountIds.Contains(r.AccountId))
            .Where(r => r.IdNavigation.Type == (int)MatterTypes.Rebate) // Filter by MatterType for safety
            .Where(r => r.IdNavigation.StatedOn >= fromUtc && r.IdNavigation.StatedOn <= toUtc)
            .Where(r => r.CurrencyId == 840 || r.CurrencyId == 841) // USD or USC
            .Where(r => r.IdNavigation.StateId == RebateCompletedState) // Only completed rebates
            .GroupBy(r => r.Account.ReferPath)
            .Select(g => new
            {
                ReferPath = g.Key,
                TotalAmount = g.Sum(r => r.CurrencyId == 840 ? r.Amount : r.Amount * 100)
            })
            .ToListAsync();

        // 7. Get trade data from MT5 deals (Mt5Deals2025s) — single source of truth
        // Aligns with Daily Equity Report (lots) and Sales Center trade page (which read MT5 directly).
        // Replaces the old _TradeRebate query, which only counted rebate-Completed rows
        // and excluded HasNoRebate (-2) / Created / etc. — i.e. real MT5 trades that didn't generate a rebate.
        var mt5Aggregates = await GetMt5TradeAggregatesAsync(descendantAccountIds, fromUtc, toUtc);

        var symbolsDict = mt5Aggregates.SymbolsByAccountId;

        // 8. Get time series data (passes mt5Aggregates so trades/lots come from the same source as summary)
        var timeSeriesData = await GetTimeSeriesDataAsync(criteria, descendantAccountIds, descendantPartyIds, fromUtc, toUtc, mt5Aggregates);

        // 9. Get product distribution from MT5 aggregates (top 8 by trade count)
        var productDist = mt5Aggregates.CountBySymbol
            .Select(kv => new { Symbol = kv.Key, Count = kv.Value })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToList();

        var totalProductTrades = productDist.Sum(x => x.Count);
        var productDistribution = productDist.Select(x => new SalesStatistics.ProductDistribution
        {
            Symbol = x.Symbol,
            Count = x.Count,
            Percentage = totalProductTrades > 0 ? Math.Round((double)x.Count / totalProductTrades * 100, 1) : 0
        }).ToList();

        // 10. Build hierarchy tree
        // Create dictionaries keyed by AccountId (not PartyId) to avoid duplicate values for accounts sharing PartyId
        var depositDict = deposits.ToDictionary(x => x.AccountId, x => x.TotalAmount);
        
        // Merge account withdrawals and wallet withdrawals by AccountId
        var withdrawalDict = withdrawalsByAccount
            .Concat(withdrawalsByWallet)
            .GroupBy(x => x.AccountId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

        // Build a per-account trade dictionary in the shape consumed by BuildHierarchyTree
        // (TradeCount = int, TotalVolume = number convertible to double via /10000 = lots).
        var tradeDict = mt5Aggregates.CountByAccountId.Keys
            .ToDictionary(
                accountId => accountId,
                accountId => (dynamic)new
                {
                    TradeCount = mt5Aggregates.CountByAccountId.GetValueOrDefault(accountId, 0),
                    // Already pre-divided to "lots * 10000.0" so /10000.0 in BuildHierarchyTree yields lots
                    TotalVolume = mt5Aggregates.VolumeByAccountId.GetValueOrDefault(accountId, 0.0)
                }
            );
        var rebateByPath = rebates.ToDictionary(x => x.ReferPath, x => x.TotalAmount);

        var hierarchyData = BuildHierarchyTree(
            descendantAccounts.Cast<dynamic>().ToList(), 
            descendantRoleNames,
            depositDict, 
            withdrawalDict,
            tradeDict,
            symbolsDict,
            rebateByPath,
            salesAccount.Uid
        );

        // 11. Calculate summary statistics
        // Include both account withdrawals (SourceAccountId) and wallet withdrawals (SourceWalletId)
        var totalWithdrawal = withdrawalsByAccount.Sum(x => x.TotalAmount) + withdrawalsByWallet.Sum(x => x.TotalAmount);
        var summaryStats = new SalesStatistics.SummaryStats
        {
            TotalTrades = mt5Aggregates.TotalCount,
            TotalDeposit = deposits.Sum(x => x.TotalAmount),
            TotalWithdrawal = totalWithdrawal,
            TotalNetDeposit = deposits.Sum(x => x.TotalAmount) - totalWithdrawal,
            TotalRebate = rebates.Sum(x => x.TotalAmount),
            TotalLots = mt5Aggregates.TotalVolume / 10000.0 // Convert raw MT5 Volume to lots
        };

        return new SalesStatistics.ResponseModel
        {
            HierarchyData = hierarchyData,
            TimeSeriesData = timeSeriesData,
            SummaryStats = summaryStats,
            ProductDistribution = productDistribution
        };
    }

    private async Task<List<SalesStatistics.TimeSeriesData>> GetTimeSeriesDataAsync(
        SalesStatistics.Criteria criteria, 
        List<long> accountIds,
        List<long> partyIds,
        DateTime fromUtc, 
        DateTime toUtc,
        Mt5TradeAggregates mt5Aggregates)
    {
        // Generate date range
        var dateRange = new List<DateTime>();
        for (var date = fromUtc.Date; date <= toUtc.Date; date = date.AddDays(1))
        {
            dateRange.Add(date);
        }

        // Get daily deposits
        // Only include completed deposits: DepositCompleted (350) or DepositCallbackCompleted (345)
        var dailyDeposits = await tenantDbContext.Deposits
            .Where(d => accountIds.Contains(d.TargetAccountId!.Value))
            .Where(d => d.IdNavigation.Type == (int)MatterTypes.Deposit) // Filter by MatterType for safety
            .Where(d => d.IdNavigation.StatedOn >= fromUtc && d.IdNavigation.StatedOn <= toUtc)
            .Where(d => d.CurrencyId == 840 || d.CurrencyId == 841)
            .Where(d => DepositCompletedStates.Contains(d.IdNavigation.StateId)) // Only completed deposits (money in wallet)
            .GroupBy(d => d.IdNavigation.StatedOn.Date)
            .Select(g => new
            {
                Date = g.Key,
                Amount = g.Sum(d => d.CurrencyId == 840 ? d.Amount : d.Amount * 100)
            })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        // Get daily withdrawals
        // Include both account withdrawals (SourceAccountId) and wallet withdrawals (SourceWalletId)
        // Only include completed withdrawals: WithdrawalCompleted (450)
        var dailyWithdrawalsByAccount = await tenantDbContext.Withdrawals
            .Where(w => w.SourceAccountId.HasValue && accountIds.Contains(w.SourceAccountId.Value))
            .Where(w => w.IdNavigation.Type == (int)MatterTypes.Withdrawal) // Filter by MatterType for safety
            .Where(w => w.IdNavigation.StatedOn >= fromUtc && w.IdNavigation.StatedOn <= toUtc)
            .Where(w => w.CurrencyId == 840 || w.CurrencyId == 841)
            .Where(w => w.IdNavigation.StateId == WithdrawalCompletedState) // Only completed withdrawals (final state)
            .GroupBy(w => w.IdNavigation.StatedOn.Date)
            .Select(g => new
            {
                Date = g.Key,
                Amount = g.Sum(w => w.CurrencyId == 840 ? w.Amount : w.Amount * 100)
            })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        // Get wallet withdrawals - match the same logic as main query (group by account, then sum)
        // Note: If multiple accounts share same PartyId/WalletId, withdrawals will be counted per account to match main query behavior
        var dailyWithdrawalsByWalletRaw = await (from w in tenantDbContext.Withdrawals
            join a in tenantDbContext.Accounts on w.PartyId equals a.PartyId
            where w.SourceWalletId.HasValue
               && w.SourceWalletId == a.WalletId
               && w.IdNavigation.Type == (int)MatterTypes.Withdrawal // Filter by MatterType for safety
               && w.IdNavigation.StatedOn >= fromUtc && w.IdNavigation.StatedOn <= toUtc
               && (w.CurrencyId == 840 || w.CurrencyId == 841)
               && w.IdNavigation.StateId == WithdrawalCompletedState // Only completed withdrawals (final state)
               && accountIds.Contains(a.Id) // Only accounts that are descendants
            select new
            {
                Date = w.IdNavigation.StatedOn.Date,
                Amount = w.CurrencyId == 840 ? w.Amount : w.Amount * 100
            })
            .ToListAsync();

        // Group by date and sum (matching the behavior of main query where withdrawals are counted per account)
        var dailyWithdrawalsByWallet = dailyWithdrawalsByWalletRaw
            .GroupBy(x => x.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Merge account and wallet withdrawals by date
        var dailyWithdrawals = dailyWithdrawalsByAccount.Keys
            .Union(dailyWithdrawalsByWallet.Keys)
            .ToDictionary(
                date => date,
                date => dailyWithdrawalsByAccount.GetValueOrDefault(date, 0) + dailyWithdrawalsByWallet.GetValueOrDefault(date, 0)
            );

        // Get daily rebates
        // Only include completed rebates: RebateCompleted (550)
        var dailyRebates = await tenantDbContext.Rebates
            .Where(r => accountIds.Contains(r.AccountId))
            .Where(r => r.IdNavigation.Type == (int)MatterTypes.Rebate) // Filter by MatterType for safety
            .Where(r => r.IdNavigation.StatedOn >= fromUtc && r.IdNavigation.StatedOn <= toUtc)
            .Where(r => r.CurrencyId == 840 || r.CurrencyId == 841)
            .Where(r => r.IdNavigation.StateId == RebateCompletedState) // Only completed rebates
            .GroupBy(r => r.IdNavigation.StatedOn.Date)
            .Select(g => new
            {
                Date = g.Key,
                Amount = g.Sum(r => r.CurrencyId == 840 ? r.Amount : r.Amount * 100)
            })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        // Daily trades come from MT5 deal aggregates already fetched in the main method.
        // Source: Mt5Deals2025s, filter Entry IN (1,2,3) AND Action IN (0,1) — same as Daily Equity Report.
        var dailyTradeCount = mt5Aggregates.CountByDate;
        var dailyTradeVolume = mt5Aggregates.VolumeByDate;

        // Build time series
        return dateRange.Select(date =>
        {
            var deposit = dailyDeposits.GetValueOrDefault(date, 0);
            var withdrawal = dailyWithdrawals.GetValueOrDefault(date, 0);

            return new SalesStatistics.TimeSeriesData
            {
                Date = date.ToString("MM-dd"),
                Trades = dailyTradeCount.GetValueOrDefault(date, 0),
                Deposit = deposit,
                Withdrawal = withdrawal,
                NetDeposit = deposit - withdrawal,
                Rebate = dailyRebates.GetValueOrDefault(date, 0),
                Lots = dailyTradeVolume.GetValueOrDefault(date, 0.0) / 10000.0
            };
        }).ToList();
    }

    /// <summary>
    /// Aggregates MT5 trade data (Mt5Deals2025s) for the given descendant accounts and date range.
    /// 
    /// Why MT5 (not _TradeRebate): the Postgres _TradeRebate table is a rebate ledger — many real
    /// MT5 trades end up with Status = HasNoRebate (-2) when no rebate rule applies, which means
    /// the old query under-counted both trade volume and lot totals. Daily Equity Report and
    /// the Sales Center trade page both read MT5 directly, so this aligns the three surfaces.
    /// 
    /// Filter (matches ReportService.DailyEquity.cs lots query):
    ///   - Entry IN (1=Out, 2=InOut/reverse, 3=OutBy/hedge close) — closing transactions only
    ///   - Action IN (0=Buy, 1=Sell) — excludes balance/credit adjustments
    /// 
    /// Lots conversion: Volume / 10000.0 (raw MT5 volume scale).
    /// </summary>
    private async Task<Mt5TradeAggregates> GetMt5TradeAggregatesAsync(
        List<long> descendantAccountIds,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var aggregates = new Mt5TradeAggregates();

        // Resolve (AccountId, AccountNumber, ServiceId) for descendants that have a TradeAccount.
        // Sales (Role 100) and IB/Agent (Role 300) accounts have no TradeAccount and are skipped here;
        // their hierarchy aggregation still works because BuildHierarchyTree rolls up from descendants.
        var tradeAccounts = await tenantDbContext.Accounts
            .Where(x => descendantAccountIds.Contains(x.Id))
            .Where(x => x.TradeAccount != null)
            .Where(x => x.IsClosed == 0)
            .Select(x => new
            {
                x.Id,
                x.AccountNumber,
                ServiceId = x.TradeAccount!.ServiceId
            })
            .ToListAsync();

        if (tradeAccounts.Count == 0)
            return aggregates;

        foreach (var serviceGroup in tradeAccounts.GroupBy(x => x.ServiceId))
        {
            var serviceId = serviceGroup.Key;

            // Skip services that aren't registered (e.g. removed) and non-MT5 platforms (MT4, etc.).
            // MT4 trade aggregation can be added later if needed by mirroring this block against Mt4Trades.
            if (!myDbContextPool.IsServiceExisted(serviceId))
            {
                logger.LogWarning("[SalesStatistics MT5] ServiceId={ServiceId} not registered. Skipping.", serviceId);
                continue;
            }

            if (myDbContextPool.GetPlatformByServiceId(serviceId) != PlatformTypes.MetaTrader5)
            {
                logger.LogInformation("[SalesStatistics MT5] ServiceId={ServiceId} is not MT5. Skipping (only MT5 trades are aggregated).", serviceId);
                continue;
            }

            // Build login -> AccountId mapping; AccountNumber is stored as long but MT5 Login is ulong
            var loginToAccountId = serviceGroup
                .Where(x => x.AccountNumber > 0)
                .ToDictionary(x => (ulong)x.AccountNumber, x => x.Id);

            if (loginToAccountId.Count == 0)
                continue;

            var logins = loginToAccountId.Keys.ToList();

            try
            {
                await using var mt5Ctx = myDbContextPool.CreateCentralMT5DbContextAsync(serviceId);

                // Pull only the columns we need; use Time (matches Daily Equity Report convention).
                // toUtc is end-of-day (23:59:59.9999999) so <= is inclusive of the same day.
                var deals = await mt5Ctx.Mt5Deals2025s
                    .Where(d => logins.Contains(d.Login))
                    .Where(d => d.Time >= fromUtc && d.Time <= toUtc)
                    .Where(d => Mt5ClosingEntries.Contains(d.Entry))
                    .Where(d => Mt5BuySellActions.Contains(d.Action))
                    .Select(d => new
                    {
                        d.Login,
                        d.Symbol,
                        d.Time,
                        d.Volume
                    })
                    .ToListAsync();

                logger.LogInformation(
                    "[SalesStatistics MT5] ServiceId={ServiceId}, Logins={LoginCount}, Range={From:yyyy-MM-dd}..{To:yyyy-MM-dd}, Deals={DealCount}",
                    serviceId, logins.Count, fromUtc, toUtc, deals.Count);

                foreach (var deal in deals)
                {
                    if (!loginToAccountId.TryGetValue(deal.Login, out var accountId))
                        continue;

                    var volume = (double)deal.Volume;
                    var dayKey = deal.Time.Date;

                    // Per-account
                    aggregates.CountByAccountId[accountId] = aggregates.CountByAccountId.GetValueOrDefault(accountId, 0) + 1;
                    aggregates.VolumeByAccountId[accountId] = aggregates.VolumeByAccountId.GetValueOrDefault(accountId, 0.0) + volume;
                    if (!aggregates.SymbolsByAccountId.TryGetValue(accountId, out var syms))
                    {
                        syms = new List<string>();
                        aggregates.SymbolsByAccountId[accountId] = syms;
                    }
                    if (!syms.Contains(deal.Symbol))
                        syms.Add(deal.Symbol);

                    // Per-day
                    aggregates.CountByDate[dayKey] = aggregates.CountByDate.GetValueOrDefault(dayKey, 0) + 1;
                    aggregates.VolumeByDate[dayKey] = aggregates.VolumeByDate.GetValueOrDefault(dayKey, 0.0) + volume;

                    // Per-symbol
                    aggregates.CountBySymbol[deal.Symbol] = aggregates.CountBySymbol.GetValueOrDefault(deal.Symbol, 0) + 1;

                    // Totals
                    aggregates.TotalCount++;
                    aggregates.TotalVolume += volume;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "[SalesStatistics MT5] Failed to aggregate trades for ServiceId={ServiceId}. Returning partial data.",
                    serviceId);
            }
        }

        return aggregates;
    }

    private List<SalesStatistics.HierarchyNode> BuildHierarchyTree(
        List<dynamic> accounts,
        Dictionary<long, string> roleNames,
        Dictionary<long, long> depositDict,
        Dictionary<long, long> withdrawalDict,
        Dictionary<long, dynamic> tradeDict,
        Dictionary<long, List<string>> symbolsDict,
        Dictionary<string, long> rebateByPath,
        long salesUid)
    {
        // Build nodes dictionary
        var nodeDict = new Dictionary<long, SalesStatistics.HierarchyNode>();
        // Store original values for IB nodes before they aggregate (to avoid double-counting when Sales aggregates)
        var ibNodeOriginalValues = new Dictionary<long, (long Deposit, long Withdrawal, int Trades, double Lots, long Rebate, List<string> Products)>();

        foreach (var account in accounts)
        {
            long uid = account.Uid;
            long partyId = account.PartyId;
            long id = account.Id;
            long role = account.Role;
            string name = account.Name ?? "";
            string group = account.Group;
            string referPath = account.ReferPath;
            
            var deposit = depositDict.GetValueOrDefault(id, 0);
            var withdrawal = withdrawalDict.GetValueOrDefault(id, 0);
            var trades = 0;
            var lots = 0.0;
            var rebate = rebateByPath.GetValueOrDefault(referPath, 0);
            var products = symbolsDict.GetValueOrDefault(id, new List<string>());

            // Get trade data if exists
            if (tradeDict.TryGetValue(id, out dynamic? trade))
            {
                if (trade != null)
                {
                    trades = (int)trade.TradeCount;
                    lots = (double)trade.TotalVolume / 10000.0;
                }
            }

            // Store original values for IB nodes before aggregation
            if (role == (int)AccountRoleTypes.Agent) // IB/Agent role = 300
            {
                ibNodeOriginalValues[uid] = (deposit, withdrawal, trades, lots, rebate, products);
            }
            
            var node = new SalesStatistics.HierarchyNode
            {
                Id = uid.ToString(),
                Name = name,
                Type = role,
                TypeName = roleNames.GetValueOrDefault(role, "Unknown"),
                GroupCode = group,
                // Use AccountId for deposits/withdrawals (not PartyId) to avoid duplicate values
                Deposit = deposit,
                // Include both account withdrawals (SourceAccountId) and wallet withdrawals (SourceWalletId)
                Withdrawal = withdrawal,
                Trades = trades,
                Lots = lots,
                Rebate = rebate,
                Products = products
            };

            nodeDict[uid] = node;
        }

        // Build parent-child relationships
        foreach (var account in accounts)
        {
            long uid = account.Uid;
            string referPath = account.ReferPath;
            
            var referPathParts = referPath.Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToList();

            if (referPathParts.Count > 1)
            {
                var parentUid = referPathParts[referPathParts.Count - 2];
                if (nodeDict.TryGetValue(parentUid, out var parentNode) && 
                    nodeDict.TryGetValue(uid, out var childNode))
                {
                    childNode.ParentId = parentUid.ToString();
                    parentNode.Children.Add(childNode);
                }
            }
        }

        // Calculate recursive aggregates (bottom-up)
        void AggregateRecursive(SalesStatistics.HierarchyNode node)
        {
            foreach (var child in node.Children)
            {
                AggregateRecursive(child);
            }

            if (node.Type == (int)AccountRoleTypes.Sales)
            {
                // For Sales nodes: Aggregate from ALL descendants recursively
                // This ensures Sales node values match summaryStats (which includes all descendants)
                AggregateFromAllDescendants(node, node);
                
                // Calculate NetDeposit after aggregating from all descendants
                node.NetDeposit = node.Deposit - node.Withdrawal;
            }
            else if (node.Type == (int)AccountRoleTypes.Agent) // IB/Agent role = 300
            {
                // For IB nodes: Aggregate from direct children only (not all descendants)
                // IB nodes show their own values plus their direct children's values
                foreach (var child in node.Children)
                {
                    // Skip Sales nodes (they are separate hierarchies)
                    if (child.Type == (int)AccountRoleTypes.Sales)
                    {
                        continue;
                    }
                    
                    node.Trades += child.Trades;
                    node.Lots += child.Lots;
                    node.Rebate += child.Rebate;
                    node.Deposit += child.Deposit;
                    node.Withdrawal += child.Withdrawal;
                    node.Products = node.Products.Union(child.Products).ToList();
                }
                
                // Calculate NetDeposit after aggregating from direct children
                node.NetDeposit = node.Deposit - node.Withdrawal;
            }
            else
            {
                // For Client nodes: Calculate NetDeposit from own values only
                node.NetDeposit = node.Deposit - node.Withdrawal;
            }
        }

        // Helper function to recursively aggregate from ALL descendants for Sales nodes
        // Sales aggregates from all descendants (IB and Client nodes) to match summaryStats
        // For IB nodes: use original values (before aggregation) to avoid double-counting
        void AggregateFromAllDescendants(SalesStatistics.HierarchyNode salesNode, SalesStatistics.HierarchyNode currentNode)
        {
            foreach (var child in currentNode.Children)
            {
                // If child is Sales, skip it (Sales nodes are separate hierarchies)
                if (child.Type == (int)AccountRoleTypes.Sales)
                {
                    continue;
                }
                
                // For IB nodes: use original values (before aggregation) to avoid double-counting
                // IB nodes already have their children aggregated, so we need to use their original values
                if (child.Type == (int)AccountRoleTypes.Agent && ibNodeOriginalValues.TryGetValue(long.Parse(child.Id), out var originalValues))
                {
                    // Use original values for IB node itself
                    salesNode.Trades += originalValues.Trades;
                    salesNode.Lots += originalValues.Lots;
                    salesNode.Rebate += originalValues.Rebate;
                    salesNode.Deposit += originalValues.Deposit;
                    salesNode.Withdrawal += originalValues.Withdrawal;
                    salesNode.Products = salesNode.Products.Union(originalValues.Products).ToList();
                    
                    // Then recursively aggregate from IB node's children
                    AggregateFromAllDescendants(salesNode, child);
                }
                else
                {
                    // For Client nodes: aggregate their own values (they don't have children aggregated)
                    salesNode.Trades += child.Trades;
                    salesNode.Lots += child.Lots;
                    salesNode.Rebate += child.Rebate;
                    salesNode.Deposit += child.Deposit;
                    salesNode.Withdrawal += child.Withdrawal;
                    salesNode.Products = salesNode.Products.Union(child.Products).ToList();
                    
                    // Recursively aggregate from deeper descendants (grandchildren, great-grandchildren, etc.)
                    AggregateFromAllDescendants(salesNode, child);
                }
            }
        }

        // Find the sales node (should be the only root node)
        var salesNode = nodeDict.GetValueOrDefault(salesUid);
        
        if (salesNode == null)
        {
            return new List<SalesStatistics.HierarchyNode>();
        }
        
        // Aggregate data recursively starting from the sales node
        AggregateRecursive(salesNode);

        // Return only the sales node as the single root
        return new List<SalesStatistics.HierarchyNode> { salesNode };
    }
}

