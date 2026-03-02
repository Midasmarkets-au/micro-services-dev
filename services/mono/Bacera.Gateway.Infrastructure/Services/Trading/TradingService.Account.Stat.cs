using Bacera.Gateway.DTO;
using Bacera.Gateway.Services.Email.ViewModel;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

public partial class TradingService
{
    public async Task<AccountStat.ParentModel> GetParentAccountStatAsync(long accountId,
        DateTime? from = null,
        DateTime? to = null)
    {
        from = from != null ? DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc) : DateTime.MinValue;
        to = to != null ? DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc) : DateTime.MaxValue;

        var itemTask = dbContext.AccountStats
            .Where(x => x.AccountId == accountId)
            .Where(x => x.Date >= from)
            .Where(x => x.Date <= to)
            .ToGroupedParentModel()
            .FirstOrDefaultAsync();
        var tradeBySymbolTask = dbConnection.ToListAsync<AccountStat.SymbolStat>(
            """
            select symbol_key as "Symbol",
                sum((ast."TradeSymbol"::jsonb -> symbol_key ->> 'total_trades')::bigint) as "TotalTrade",
                sum((ast."TradeSymbol"::jsonb -> symbol_key ->> 'total_profit')::bigint) as "TotalProfit",
                sum((ast."TradeSymbol"::jsonb -> symbol_key ->> 'total_volume')::bigint) as "TotalVolume"
                
            from trd."_AccountStat" ast, jsonb_object_keys(ast."TradeSymbol"::jsonb) as symbol_key
            where "AccountId" = @accountId and "Date" >= @from and "Date" <= @to
            group by symbol_key;
            """
            , new { accountId, from, to });

        var item = await itemTask;
        if (item == null) return new AccountStat.ParentModel();
        item.TradeBySymbol = (await tradeBySymbolTask).ToList();
        if (from != DateTime.MinValue) item.From = from;
        if (to != DateTime.MaxValue) item.To = to;
        return item;
    }

    public async Task<List<Account.ChildNetStatAmountResponseModel>> GetDirectChildNetAmountForAccountById(
        long parentAccountId, DateTime? from = null, DateTime? to = null, bool viewClient = false)
    {
        var parentAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == parentAccountId);
        if (parentAccount == null)
        {
            return new List<Account.ChildNetStatAmountResponseModel>();
        }

        var result = await GetDirectChildNetAmountForAccount(parentAccount, from, to);
        if (parentAccount.Role == (int)AccountRoleTypes.Rep)
            return result;

        return viewClient
            ? result.Where(x => x.Role == AccountRoleTypes.Client).ToList()
            : result.Where(x => x.Role != AccountRoleTypes.Client).ToList();
    }

    public async Task<List<Account.ChildNetStatAmountResponseModel>> GetDirectChildNetAmountForAccountByUid(
        long parentAccountUid, DateTime? from = null, DateTime? to = null, bool viewClient = false)
    {
        var parentAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Uid == parentAccountUid);
        if (parentAccount == null) return [];

        var result = await GetDirectChildNetAmountForAccount(parentAccount, from, to);
        if (parentAccount.Role == (int)AccountRoleTypes.Rep)
            return result;

        return viewClient
            ? result.Where(x => x.Role == AccountRoleTypes.Client).ToList()
            : result.ToList();
    }


    // how much the agent rebate generated from trading => agent uid
    public async Task<Dictionary<string, Account.ChildRebateStatAmountResponseModel.SymbolDict>>
        GetChildAccountRebateSymbolGroupedStatByUid(long agentUid, DateTime? from = null, DateTime? to = null)
    {
        var account = await dbContext.Accounts.Where(x => x.Uid == agentUid)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync();

        if (account == null)
        {
            return new Dictionary<string, Account.ChildRebateStatAmountResponseModel.SymbolDict>();
        }

        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var infoList = await dbContext.AccountRebateViews
            .Where(x => x.AccountId == account.Id)
            .Where(x => from == null || x.StatedOn >= from.Value)
            .Where(x => to == null || x.StatedOn <= to.Value)
            .Select(x => new AcctStatistics
            {
                Amount = x.Amount,
                Volume = x.Volume,
                Ticket = x.Ticket,
                Symbol = x.Symbol,
                Profit = x.Profit,
                CurrencyId = x.CurrencyId,
                // GroupReferPath = x.ReferPath.Substring(0, directChildReferPathLength),
            })
            .ToListAsync();

        var summedUpVolumeBySymbol = infoList
            .DistinctBy(x => x.Ticket)
            .GroupBy(x => x.Symbol)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(y => y.Volume)
            );

        return infoList.GroupBy(y => y.Symbol)
            .Select(y => new
            {
                y.Key,
                // Volume = y.Sum(z => z.Volume),
                Prifit = y.Sum(z => z.Profit),
                AmountsByCurrency = y
                    .GroupBy(z => z.CurrencyId)
                    .ToDictionary(
                        z => z.Key,
                        z => z.Sum(w => w.Amount)
                    )
            })
            .ToDictionary(
                y => y.Key,
                y => new Account.ChildRebateStatAmountResponseModel.SymbolDict
                {
                    Volume = summedUpVolumeBySymbol.GetValueOrDefault(y.Key, 0),
                    Profit = Math.Round(y.Prifit, 2),
                    Amounts = y.AmountsByCurrency
                }
            );
    }

    public async Task<IEnumerable<TradeStatDTO.SymbolCurrencyDTO>> GetChildAccountTradeSymbolGroupedStatByUid(long parentUid, DateTime? from = null,
        DateTime? to = null)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Uid == parentUid);
        if (account == null) return [];

        var referPath = account.ReferPath;
        var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var infoList = await dbContext.AccountTradeViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => from == null || x.OpenedOn >= from.Value)
            .Where(x => to == null || x.ClosedOn <= to.Value)
            .Where(x => x.CurrencyId > 0) // exclude imported old trades
            .Select(x => new AcctStatistics
            {
                Amount = x.Amount,
                Volume = x.Volume,
                Symbol = x.Symbol,
                CurrencyId = x.CurrencyId,
                GroupReferPath = x.ReferPath.Substring(0, directChildReferPathLength),
            })
            .ToListAsync();

        var result = infoList.GroupBy(y => new { y.CurrencyId, y.Symbol })
            .Select(y => new TradeStatDTO.SymbolCurrencyDTO
            {
                Symbol = y.Key.Symbol,
                CurrencyId = (CurrencyTypes)y.Key.CurrencyId,
                Volume = y.Sum(z => z.Volume),
                Profit = y.Sum(z => z.Amount)
            }).OrderBy(x => x.Symbol, StringComparer.Ordinal);
        return result;
    }

    public async Task<int> GetNewAccountCountByUid(Account account, DateTime? from = null, DateTime? to = null)
    {
        var referPath = account.ReferPath;
        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var newAccountCount = await dbContext.Accounts
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.Role == (int)AccountRoleTypes.Client)
            .Where(x => x.CreatedOn >= from)
            .Where(x => x.CreatedOn <= to)
            .CountAsync();

        return newAccountCount;
    }

    public async Task<int> GetNewAgentCountByUid(Account account, DateTime? from = null, DateTime? to = null)
    {
        var referPath = account.ReferPath;
        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var newIbCount = await dbContext.Accounts
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.Role == (int)AccountRoleTypes.Agent)
            .Where(x => x.CreatedOn >= from)
            .Where(x => x.CreatedOn <= to)
            .CountAsync();

        return newIbCount;
    }

    public async Task<int> GetRebateAmountByUid(Account account, DateTime? from = null, DateTime? to = null)
    {
        var referPath = account.ReferPath;
        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var rebateAmount = await dbContext.AccountRebateViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.StatedOn >= from)
            .Where(x => x.StatedOn <= to)
            .SumAsync(x => x.Amount);

        return (int)rebateAmount;
    }

    public async Task<int> GetDepositAmountByUid(Account account, DateTime? from = null, DateTime? to = null)
    {
        var referPath = account.ReferPath;
        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var depositAmount = await dbContext.AccountDepositViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.StatedOn >= from)
            .Where(x => x.StatedOn <= to)
            .SumAsync(x => x.Amount);

        return (int)depositAmount;
    }

    public async Task<int> GetWithdrawAmountByUid(Account account, DateTime? from = null, DateTime? to = null)
    {
        var referPath = account.ReferPath;
        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var withdrawAmount = await dbContext.AccountWithdrawalViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.StatedOn >= from)
            .Where(x => x.StatedOn <= to)
            .SumAsync(x => x.Amount);

        return (int)withdrawAmount;
    }

    public async Task<dynamic> GetTradeSymbolByUid(Account account, DateTime? from = null, DateTime? to = null)
    {
        var referPath = account.ReferPath;
        // var directChildReferPathLength = referPath.Length + account.Uid.ToString().Length + 1;

        var tradeSymbols = await dbContext.AccountTradeViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.ClosedOn > from)
            .Where(x => x.ClosedOn < to)
            .GroupBy(x => x.Symbol)
            .Select(group => new
            {
                Symbol = group.Key, TotalVolume = group.Sum(trade => trade.Volume),
                TotalPorfit = group.Sum(trade => trade.Amount)
            })
            .ToListAsync();

        return tradeSymbols;
    }


    private async Task<List<Account.ChildNetStatAmountResponseModel>> GetDirectChildNetAmountForAccount(
        Account parentAccount, DateTime? from = null, DateTime? to = null)
    {
        var referPath = parentAccount.ReferPath;
        var directChildReferPathLength = referPath.Length + parentAccount.Uid.ToString().Length + 1;

        var directChildAccounts = await dbContext.Accounts
            .Where(x => x.ReferPath.StartsWith(referPath) && x.Level == parentAccount.Level + 1)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Group,
                x.Role,
                ReferPath = x.ReferPath.Substring(0, directChildReferPathLength)
            })
            .ToListAsync();

        // directChildAccounts.ForEach(x => x.ReferPath = x.ReferPath[..directChildReferPathLength]);

        var depositStatistics = await dbContext.AccountDepositViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => from == null || x.StatedOn >= from.Value)
            .Where(x => to == null || x.StatedOn <= to.Value)
            .Select(x => new AcctStatistics
            {
                Amount = x.Amount,
                CurrencyId = x.CurrencyId,
                GroupReferPath = x.ReferPath.Substring(0, directChildReferPathLength),
            })
            .ToGroupedAcctStatistics()
            .ToListAsync();
        var depositGrouped = depositStatistics.ToSummedUpGroupedAcctStatistics();

        var withdrawalStatistics = await dbContext.AccountWithdrawalViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => from == null || x.StatedOn >= from.Value)
            .Where(x => to == null || x.StatedOn <= to.Value)
            .Select(x => new AcctStatistics
            {
                Amount = x.Amount,
                CurrencyId = x.CurrencyId,
                GroupReferPath = x.ReferPath.Substring(0, directChildReferPathLength),
            })
            .ToGroupedAcctStatistics()
            .ToListAsync();
        var withdrawalGrouped = withdrawalStatistics.ToSummedUpGroupedAcctStatistics();

        var rebateInfoList = await dbContext.AccountRebateViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => x.AccountId == parentAccount.Id)
            .Where(x => from == null || x.StatedOn >= from.Value)
            .Where(x => to == null || x.StatedOn <= to.Value)
            .Select(x => new AcctStatistics
            {
                Amount = x.Amount,
                Volume = x.Volume,
                Symbol = x.Symbol,
                CurrencyId = x.CurrencyId,
                GroupReferPath = x.ReferPath.Substring(0, directChildReferPathLength),
            })
            .ToListAsync();
        var rebateGrouped = rebateInfoList.ToSummedUpGroupedAcctStatistics();

        var tradeStats = await dbContext.AccountTradeViews
            .Where(x => x.ReferPath.StartsWith(referPath))
            .Where(x => from == null || x.OpenedOn >= from.Value)
            .Where(x => to == null || x.ClosedOn <= to.Value)
            .Select(x => new AcctStatistics
            {
                Amount = x.Amount,
                Volume = x.Volume,
                Symbol = x.Symbol,
                CurrencyId = x.CurrencyId,
                GroupReferPath = x.ReferPath.Substring(0, directChildReferPathLength),
            })
            .ToListAsync();
        var tradeGrouped = tradeStats.ToSummedUpGroupedAcctStatistics();

        var results = directChildAccounts
            .Select(x => new Account.ChildNetStatAmountResponseModel
            {
                Id = x.Id,
                Uid = x.Uid,
                Group = x.Group,
                Role = (AccountRoleTypes)x.Role,
                DepositAmounts = depositGrouped.TryGetValue(x.ReferPath, out var deposit)
                    ? deposit
                    : new Dictionary<int, long>(),
                WithdrawalAmounts = withdrawalGrouped.TryGetValue(x.ReferPath, out var withdrawal)
                    ? withdrawal
                    : new Dictionary<int, long>(),
                RebateAmounts = rebateGrouped.TryGetValue(x.ReferPath, out var rebate)
                    ? rebate
                    : new Dictionary<int, long>(),
                ProfitAmounts = tradeGrouped.TryGetValue(x.ReferPath, out var trade)
                    ? trade
                    : new Dictionary<int, long>()
            })
            .ToList();

        var clientAccounts = directChildAccounts.Where(x => x.Role == (int)AccountRoleTypes.Client).ToList();

        var summedUpDeposits = clientAccounts
            .SelectMany(account => depositGrouped.TryGetValue(account.ReferPath, out var deposits)
                ? deposits
                : new Dictionary<int, long>())
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Sum(pair => pair.Value));

        var summedUpWithdrawal = clientAccounts
            .SelectMany(account => withdrawalGrouped.TryGetValue(account.ReferPath, out var withdrawals)
                ? withdrawals
                : new Dictionary<int, long>())
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Sum(pair => pair.Value));

        results.Add(new Account.ChildNetStatAmountResponseModel
        {
            Id = 0,
            Uid = 0,
            Group = "Client-Sum-Up",
            Role = AccountRoleTypes.Agent,
            DepositAmounts = summedUpDeposits,
            WithdrawalAmounts = summedUpWithdrawal
        });
        var sortedResults = results
            .OrderByDescending(x => x.DepositAmounts.Count)
            .ThenBy(x => x.DepositAmounts.Sum(y => y.Value))
            .ToList();

        return sortedResults;
    }
}

public class AcctStatistics
{
    public long Amount { get; set; }
    public long Ticket { get; set; }
    public int CurrencyId { get; set; }
    public long Volume { get; set; }
    public double Profit { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string GroupReferPath { get; set; } = null!;
}

public static class StatisticsExt
{
    public static IQueryable<AcctStatistics> ToGroupedAcctStatistics(this IQueryable<AcctStatistics> query)
        => query.GroupBy(x => new { x.GroupReferPath, x.CurrencyId })
            .Select(x => new AcctStatistics
            {
                Amount = x.Sum(y => y.Amount),
                CurrencyId = x.Key.CurrencyId,
                GroupReferPath = x.Key.GroupReferPath
            });

    public static Dictionary<string, Dictionary<int, long>> ToSummedUpGroupedAcctStatistics(
        this IEnumerable<AcctStatistics> query)
        => query.GroupBy(x => x.GroupReferPath)
            .ToDictionary(
                x => x.Key,
                x => x.GroupBy(y => y.CurrencyId)
                    .ToDictionary(
                        y => y.Key,
                        y => y.Sum(z => z.Amount)
                    )
            );

    public static Dictionary<string, Dictionary<string, Account.ChildRebateStatAmountResponseModel.SymbolDict>>
        ToSymbolGroupedAcctStatistics(
            this IEnumerable<AcctStatistics> query)
        => query.GroupBy(x => x.GroupReferPath)
            .ToDictionary(
                x => x.Key,
                x => x
                    .GroupBy(y => new { y.Symbol, y.CurrencyId })
                    .Select(y => new AcctStatistics
                    {
                        Symbol = y.Key.Symbol,
                        CurrencyId = y.Key.CurrencyId,
                        Volume = y.Sum(z => z.Volume),
                        Amount = y.Sum(z => z.Amount)
                    })
                    .GroupBy(y => y.Symbol)
                    .Select(y => new
                    {
                        y.Key,
                        Volume = y.Sum(z => z.Volume),
                        AmountsByCurrency = y
                            .GroupBy(z => z.CurrencyId)
                            .ToDictionary(
                                z => z.Key,
                                z => z.Sum(w => w.Amount)
                            )
                    })
                    .ToDictionary(
                        y => y.Key,
                        y => new Account.ChildRebateStatAmountResponseModel.SymbolDict
                        {
                            Volume = y.Volume,
                            Amounts = y.AmountsByCurrency
                        }
                    )
            );
}