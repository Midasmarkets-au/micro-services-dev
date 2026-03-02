using Bacera.Gateway.DTO;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService
{
    public async Task<TradeStatDTO.TenantAccountStat> GetTradeStatisticsByIdAsync(List<long> accountNumbers, DateTime from, DateTime to)
    {
        var accounts = await tenantCtx.Accounts
            .Where(x => accountNumbers.Contains(x.AccountNumber))
            .Select(x => new { x.AccountNumber, x.ServiceId })
            .ToListAsync();
        if (accounts.Count == 0) return new TradeStatDTO.TenantAccountStat();
        var serviceIds = accounts.Select(x => x.ServiceId).Distinct().ToList();
        if (serviceIds.Count != 1) throw new Exception("Accounts are not in the same service");

        var serviceId = serviceIds.First();
        var platform = pool.GetPlatformByServiceId(serviceId);
        return platform switch
        {
            PlatformTypes.MetaTrader4 => await GetMT4StatForTenant(accountNumbers, serviceId, from, to),
            PlatformTypes.MetaTrader5 => await GetMT5StatForTenant(accountNumbers, serviceId, from, to),
            _ => new TradeStatDTO.TenantAccountStat()
        };
    }

    private async Task<TradeStatDTO.TenantAccountStat> GetMT4StatForTenant(List<long> accountNumbers, int serviceId, DateTime from, DateTime to)
    {
        await using var mt4Ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
        var account = await mt4Ctx.Mt4Users
            .Where(x => accountNumbers.Contains((long)x.Login))
            .GroupBy(x => 1)
            .Select(x => new
            {
                Equity = x.Sum(y => y.Equity),
                Balance = x.Sum(y => y.Balance)
            })
            .SingleOrDefaultAsync();
        if (account == null) return new TradeStatDTO.TenantAccountStat();

        var openItem = await mt4Ctx.Mt4Trades
            .Where(x => x.OpenTime >= from && x.OpenTime <= to)
            .Where(x => x.Cmd == 0 || x.Cmd == 1)
            .Where(x => accountNumbers.Contains(x.Login))
            .GroupBy(x => new { x.Symbol, x.Cmd })
            .Select(x => new TradeStatDTO.TradeBySymbolAndCmd
            {
                Symbol = x.Key.Symbol,
                Cmd = x.Key.Cmd,
                Digits = (int)x.Average(y => y.Digits),
                AvePriceSum = x.Sum(y => (decimal)y.OpenPrice * (y.Volume / 100m)),
                Volume = Math.Round(x.Sum(y => y.Volume) / 100.0m, 2),
                Profit = Math.Round(x.Sum(y => y.Profit), 2)
            })
            .OrderBy(x => x.Symbol)
            .ThenBy(x => x.Cmd)
            .ToListAsync();

        var closedItem = await mt4Ctx.Mt4Trades
            .Where(x => x.CloseTime >= from && x.CloseTime <= to)
            .Where(x => x.Cmd == 0 || x.Cmd == 1)
            .Where(x => accountNumbers.Contains(x.Login))
            .GroupBy(x => new { x.Symbol, x.Cmd })
            .Select(x => new TradeStatDTO.TradeBySymbolAndCmd
            {
                Symbol = x.Key.Symbol,
                Cmd = x.Key.Cmd,
                Digits = (int)x.Average(y => y.Digits),
                AvePriceSum = x.Sum(y => (decimal)y.ClosePrice * (y.Volume / 100m)),
                Volume = Math.Round(x.Sum(y => y.Volume) / 100.0m, 2),
                Profit = Math.Round(x.Sum(y => y.Profit), 2)
            })
            .OrderBy(x => x.Symbol)
            .ThenBy(x => x.Cmd)
            .ToListAsync();

        var currentPositions = await mt4Ctx.Mt4Trades
            .Where(x => x.CloseTime <= Mt4Trade.DefaultDateTime)
            .Where(x => accountNumbers.Contains(x.Login))
            .Where(x => x.Cmd == 0 || x.Cmd == 1)
            .GroupBy(x => new { x.Symbol, x.Cmd })
            .Select(x => new TradeStatDTO.TradeBySymbolAndCmd
            {
                Symbol = x.Key.Symbol,
                Cmd = x.Key.Cmd,
                Digits = (int)x.Average(y => y.Digits),
                AvePriceSum = x.Sum(y => (decimal)y.OpenPrice * (y.Volume / 100m)),
                Volume = Math.Round(x.Sum(y => y.Volume) / 100.0m, 2),
                Profit = Math.Round(x.Sum(y => y.Profit), 2)
            })
            .OrderBy(x => x.Symbol)
            .ToListAsync();

        var currentPositionSymbols = currentPositions.Select(x => x.Symbol).Distinct().ToList();
        var closedItemSymbols = closedItem.Select(x => x.Symbol).Distinct().ToList();
        var symbols = currentPositionSymbols.Union(closedItemSymbols).ToList();
        var prices = await mt4Ctx.Mt4Prices
            .Where(x => symbols.Contains(x.Symbol))
            .Select(x => new TradeStatDTO.MtPrice
            {
                Symbol = x.Symbol,
                Bid = x.Bid,
                Ask = x.Ask
            })
            .ToListAsync();


        var result = new TradeStatDTO.TenantAccountStat
        {
            OpenTradeStats = openItem,
            ClosedTradeStats = closedItem,
            OpenTrades = currentPositions,
            OpenTradeCurrentPrices = prices,
            Equity = Math.Round(account.Equity, 2),
            Balance = Math.Round(account.Balance, 2)
        };

        return result;
    }

    private async Task<TradeStatDTO.TenantAccountStat> GetMT5StatForTenant(List<long> accountNumbers, int serviceId, DateTime from, DateTime to)
    {
        await using var mt5Ctx = pool.CreateCentralMT5DbContextAsync(serviceId);
        var account = await mt5Ctx.Mt5Accounts
            .Where(x => accountNumbers.Contains((long)x.Login))
            .GroupBy(x => 1)
            .Select(x => new
            {
                Equity = x.Sum(y => y.Equity),
                Balance = x.Sum(y => y.Balance)
            })
            .SingleOrDefaultAsync();
        if (account == null) return new TradeStatDTO.TenantAccountStat();

        var openItem = await mt5Ctx.Mt5Deals2025s
            .Where(x => x.TimeMsc >= from && x.TimeMsc <= to)
            .Where(x => x.Action == 0 || x.Action == 1)
            .Where(x => accountNumbers.Contains((long)x.Login))
            .Where(x => x.VolumeClosed == 0)
            .GroupBy(x => new { x.Symbol, x.Action })
            .Select(x => new TradeStatDTO.TradeBySymbolAndCmd
            {
                Symbol = x.Key.Symbol,
                Cmd = (int)x.Key.Action,
                // Replace .ToCentsFromScaled() with direct division by 10000 to avoid LINQ error when no data
                Volume = Math.Round(((decimal)x.Sum(y => (long)y.Volume)) / 10000m, 2),
                Digits = (int)x.Average(y => y.Digits),
                // Replace .ToCentsFromScaled() with direct division by 10000 to avoid LINQ error when no data
                AvePriceSum = x.Sum(y => (decimal)y.Price * ((decimal)y.Volume / 10000m)),
                Profit = Math.Round(x.Sum(y => y.Profit), 2)
            })
            .OrderBy(x => x.Symbol)
            .ThenBy(x => x.Cmd)
            .ToListAsync();

        var closedItem = await mt5Ctx.Mt5Deals2025s
            .Where(x => x.Action == 0 || x.Action == 1)
            .Where(x => x.TimeMsc >= from && x.TimeMsc <= to)
            .Where(x => x.VolumeClosed > 0)
            .Where(x => accountNumbers.Contains((long)x.Login))
            .GroupBy(x => new { x.Symbol, x.Action })
            .Select(x => new TradeStatDTO.TradeBySymbolAndCmd
            {
                Symbol = x.Key.Symbol,
                Cmd = 1 - (int)x.Key.Action,
                Digits = (int)x.Average(y => y.Digits),
                // Replace .ToCentsFromScaled() with direct division by 10000 to avoid LINQ error when no data
                AvePriceSum = x.Sum(y => (decimal)y.Price * ((decimal)y.Volume / 10000m)),
                Volume = Math.Round(((decimal)x.Sum(y => (long)y.Volume)) / 10000m, 2),
                Profit = Math.Round(x.Sum(y => y.Profit), 2)
            })
            .OrderBy(x => x.Symbol)
            .ThenBy(x => x.Cmd)
            .ToListAsync();

        var currentPositions = await mt5Ctx.Mt5Positions
            .Where(x => x.Action == 0 || x.Action == 1)
            .Where(x => accountNumbers.Contains((long)x.Login))
            .GroupBy(x => new { x.Symbol, x.Action })
            .Select(x => new TradeStatDTO.TradeBySymbolAndCmd
            {
                Symbol = x.Key.Symbol,
                Cmd = (int)x.Key.Action,
                Digits = (int)x.Average(y => y.Digits),
                // Replace .ToCentsFromScaled() with direct division by 10000 to avoid LINQ error when no data
                AvePriceSum = x.Sum(y => (decimal)y.PriceOpen * ((decimal)y.Volume / 10000m)),
                Volume = Math.Round(((decimal)x.Sum(y => (long)y.Volume)) / 10000m, 2),
                Profit = Math.Round(x.Sum(y => y.Profit), 2)
            })
            .OrderBy(x => x.Symbol)
            .ToListAsync();

        var currentPositionSymbols = currentPositions.Select(x => x.Symbol).Distinct().ToList();
        var closedItemSymbols = closedItem.Select(x => x.Symbol).Distinct().ToList();
        var symbols = currentPositionSymbols.Union(closedItemSymbols).ToList();
        var prices = await mt5Ctx.Mt5Prices
            .Where(x => symbols.Contains(x.Symbol))
            .Select(x => new TradeStatDTO.MtPrice
            {
                Symbol = x.Symbol,
                Bid = x.BidLast,
                Ask = x.AskLast
            })
            .ToListAsync();

        var result = new TradeStatDTO.TenantAccountStat
        {
            OpenTradeStats = openItem,
            ClosedTradeStats = closedItem,
            OpenTrades = currentPositions,
            OpenTradeCurrentPrices = prices,
            Equity = Math.Round(account.Equity, 2),
            Balance = Math.Round(account.Balance, 2)
        };

        return result;
    }
}