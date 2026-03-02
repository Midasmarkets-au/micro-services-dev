using Microsoft.EntityFrameworkCore;
using Bacera.Gateway.Core.Types;

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
    
    private static readonly int TradeRebateCompletedStatus = (int)TradeRebateStatusTypes.Completed;

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

        // 7. Get trade data (accountIds already includes sales account)
        // Only include completed trades: Status = Completed (2)
        // Only include Buy (0) and Sell (1) actions (exclude other action types)
        var tradeData = await (from tr in tenantDbContext.TradeRebates
            join a in tenantDbContext.Accounts on tr.AccountId equals a.Id
            where descendantAccountIds.Contains(tr.AccountId!.Value)
               && tr.ClosedOn >= fromUtc && tr.ClosedOn <= toUtc
               && tr.Status == TradeRebateCompletedStatus // Only completed trades
               && (tr.Action == 0 || tr.Action == 1) // Only Buy (0) and Sell (1) trades
               && a.IsClosed == 0 // Only active accounts (same as AccountTrade view)
            group tr by new { tr.AccountId, a.ReferPath } into g
            select new
            {
                g.Key.AccountId,
                g.Key.ReferPath,
                TradeCount = g.Count(), // Count all completed trades
                TotalVolume = g.Sum(t => t.Volume)
            })
            .ToListAsync();

        // 7b. Get symbols separately (EF Core limitation workaround)
        // Only include completed trades: Status = Completed (2)
        // Only include Buy (0) and Sell (1) actions (exclude other action types)
        // First materialize all the data, then do grouping and distinct in memory
        var allTradeSymbols = await (from tr in tenantDbContext.TradeRebates
            join a in tenantDbContext.Accounts on tr.AccountId equals a.Id
            where descendantAccountIds.Contains(tr.AccountId!.Value)
               && tr.ClosedOn >= fromUtc && tr.ClosedOn <= toUtc
               && tr.Status == TradeRebateCompletedStatus // Only completed trades
               && (tr.Action == 0 || tr.Action == 1) // Only Buy (0) and Sell (1) trades
               && a.IsClosed == 0 // Only active accounts
               && tr.AccountId.HasValue // Ensure AccountId is not null
            select new { AccountId = tr.AccountId!.Value, tr.Symbol })
            .ToListAsync();

        var symbolsDict = allTradeSymbols
            .GroupBy(t => t.AccountId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Symbol).Distinct().ToList()
            );

        // 8. Get time series data
        var timeSeriesData = await GetTimeSeriesDataAsync(criteria, descendantAccountIds, descendantPartyIds, fromUtc, toUtc);

        // 9. Get product distribution
        // Only include completed trades: Status = Completed (2)
        // Only include Buy (0) and Sell (1) actions (exclude other action types)
        var productDist = await (from tr in tenantDbContext.TradeRebates
            join a in tenantDbContext.Accounts on tr.AccountId equals a.Id
            where descendantAccountIds.Contains(tr.AccountId!.Value)
               && tr.ClosedOn >= fromUtc && tr.ClosedOn <= toUtc
               && tr.Status == TradeRebateCompletedStatus // Only completed trades
               && (tr.Action == 0 || tr.Action == 1) // Only Buy (0) and Sell (1) trades
               && a.IsClosed == 0 // Only active accounts
            group tr by tr.Symbol into g
            select new
            {
                Symbol = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync();

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
        
        // Convert nullable AccountId to non-nullable for dictionary key
        var tradeDict = tradeData
            .Where(x => x.AccountId.HasValue)
            .ToDictionary(x => x.AccountId!.Value, x => (dynamic)x);
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
            TotalTrades = tradeData.Sum(x => x.TradeCount),
            TotalDeposit = deposits.Sum(x => x.TotalAmount),
            TotalWithdrawal = totalWithdrawal,
            TotalNetDeposit = deposits.Sum(x => x.TotalAmount) - totalWithdrawal,
            TotalRebate = rebates.Sum(x => x.TotalAmount),
            TotalLots = tradeData.Sum(x => x.TotalVolume / 10000.0) // Convert to lots
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
        DateTime toUtc)
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

        // Get daily trades
        // Only include completed trades: Status = Completed (2)
        // Only include Buy (0) and Sell (1) actions (exclude other action types)
        var dailyTradesList = await (from tr in tenantDbContext.TradeRebates
            join a in tenantDbContext.Accounts on tr.AccountId equals a.Id
            where accountIds.Contains(tr.AccountId!.Value)
               && tr.ClosedOn >= fromUtc && tr.ClosedOn <= toUtc
               && tr.Status == TradeRebateCompletedStatus // Only completed trades
               && (tr.Action == 0 || tr.Action == 1) // Only Buy (0) and Sell (1) trades
               && a.IsClosed == 0 // Only active accounts
            group tr by tr.ClosedOn.Date into g
            select new
            {
                Date = g.Key,
                Count = g.Count(),
                Volume = g.Sum(t => t.Volume)
            })
            .ToListAsync();

        var dailyTrades = dailyTradesList.ToDictionary(x => x.Date, x => new { x.Count, x.Volume });

        // Build time series
        return dateRange.Select(date =>
        {
            var deposit = dailyDeposits.GetValueOrDefault(date, 0);
            var withdrawal = dailyWithdrawals.GetValueOrDefault(date, 0);

            return new SalesStatistics.TimeSeriesData
            {
                Date = date.ToString("MM-dd"),
                Trades = dailyTrades.GetValueOrDefault(date)?.Count ?? 0,
                Deposit = deposit,
                Withdrawal = withdrawal,
                NetDeposit = deposit - withdrawal,
                Rebate = dailyRebates.GetValueOrDefault(date, 0),
                Lots = (dailyTrades.GetValueOrDefault(date)?.Volume ?? 0) / 10000.0
            };
        }).ToList();
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

