using Bacera.Gateway.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway;

partial class TradingService
{
    // public async Task<int> GenerateTradeRebateAsync(TradeService tradeService)
    // {
    //     var incomingTradeRebates = tradeService.Platform switch
    //     {
    //         (int)PlatformTypes.MetaTrader4 => await FetchMt4TradeRebate(tradeService),
    //         (int)PlatformTypes.MetaTrader5 => await FetchMt5TradeRebate(tradeService),
    //         _ => new List<TradeRebate>()
    //     };
    //
    //     if (!incomingTradeRebates.Any()) return 0;
    //
    //     var insertedCount = await ProcessingIncomingTradeRebates(incomingTradeRebates);
    //     _logger.LogInformation(
    //         $"GenerateTradeRebateAsync: Inserted {insertedCount}/{incomingTradeRebates.Count} trade rebates for {tradeService.Name}.");
    //
    //     return insertedCount;
    // }

    // private async Task<int> ProcessingIncomingTradeRebates(List<TradeRebate> tradeRebates)
    // {
    //     if (!tradeRebates.Any()) return 0;
    //     var tradeServiceIds = tradeRebates.Select(x => x.TradeServiceId).Distinct().ToList();
    //     var accountNumbers = tradeRebates.Select(x => x.AccountNumber).Distinct().ToList();
    //     var tradeAccounts = await dbContext.Accounts
    //         .Where(x => tradeServiceIds.Contains(x.ServiceId))
    //         .Where(x => accountNumbers.Contains(x.AccountNumber))
    //         .ToListAsync();
    //
    //     var insertedCount = 0;
    //     foreach (var tradeRebate in tradeRebates)
    //     {
    //         var exists = await dbContext.TradeRebates
    //             .Where(x => x.TradeServiceId == tradeRebate.TradeServiceId)
    //             .Where(x => x.Ticket == tradeRebate.Ticket)
    //             .AnyAsync();
    //         if (exists) continue;
    //
    //         var tradeAccount = tradeAccounts
    //             .Where(x => x.ServiceId == tradeRebate.TradeServiceId && x.AccountNumber == tradeRebate.AccountNumber)
    //             .Select(x => new { x.Id, x.ServiceId, x.IsClosed, x.AccountNumber, x.Status, x.CurrencyId, x.SiteId, x.ReferPath })
    //             .FirstOrDefault();
    //
    //         if (tradeAccount == null)
    //             continue;
    //
    //         if (tradeAccount.IsClosed != 0 || tradeAccount.Status != (int)AccountStatusTypes.Activate)
    //             continue;
    //
    //         tradeRebate.AccountId = tradeAccount.Id;
    //         tradeRebate.CurrencyId = tradeAccount.CurrencyId;
    //         tradeRebate.AccountNumber = tradeAccount.AccountNumber;
    //         tradeRebate.Status = (int)TradeRebateStatusTypes.Created;
    //         tradeRebate.ReferPath = tradeAccount.ReferPath;
    //         await dbContext.TradeRebates.AddAsync(tradeRebate);
    //         try
    //         {
    //             await dbContext.SaveChangesAsync();
    //             dbContext.ChangeTracker.Clear();
    //             insertedCount++;
    //         }
    //         catch (Exception)
    //         {
    //             // ignored
    //         }
    //     }
    //
    //     return insertedCount;
    // }

    // private static DateTime Epoch => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //
    // private async Task<List<TradeRebate>> FetchMt4TradeRebate(TradeService tradeService)
    // {
    //     var lastRebateDate = await dbContext.TradeRebates
    //         .Where(x => x.TradeServiceId == tradeService.Id)
    //         .MaxAsync(x => (DateTime?)x.ClosedOn);
    //
    //     var ctx = await GetMetaTrade4DbContext(tradeService.Id);
    //
    //     return await ctx.Mt4Trades
    //         .Where(x => x.CloseTime >= lastRebateDate && x.CloseTime > Epoch)
    //         .Where(x => x.Cmd == 0 || x.Cmd == 1)
    //         .OrderBy(x => x.CloseTime)
    //         .ToTradeRebate(tradeService.Id)
    //         .ToListAsync();
    // }
    //
    // private async Task<List<TradeRebate>> FetchMt5TradeRebate(TradeService tradeService)
    // {
    //     var closeFrom = await dbContext.TradeRebates
    //         .Where(x => x.TradeServiceId == tradeService.Id)
    //         .MaxAsync(x => (DateTime?)x.ClosedOn);
    //
    //     var hasElements = await dbContext.TradeRebates
    //         .AnyAsync(x => x.TradeServiceId == tradeService.Id && x.ClosedOn >= closeFrom);
    //
    //     var maxDealId = 0L;
    //     if (hasElements)
    //     {
    //         maxDealId = await dbContext.TradeRebates
    //             .Where(x => x.TradeServiceId == tradeService.Id)
    //             .Where(x => x.ClosedOn >= closeFrom)
    //             .MaxAsync(x => x.DealId);
    //     }
    //
    //     var ctx = await GetMetaTrade5DbContext(tradeService.Id);
    //
    //     var tradeRebates = await ctx.Mt5Deals
    //         .Where(x => x.Deal > (ulong)maxDealId)
    //         .Where(x => x.VolumeClosed > 0)
    //         .Where(x => x.TimeMsc >= closeFrom)
    //         .Where(x => x.Action == 0 || x.Action == 1)
    //         .OrderBy(x => x.Deal)
    //         .ToTradeRebate(tradeService.Id)
    //         .ToListAsync();
    //
    //     await FulfillOpen(tradeRebates, ctx);
    //     return tradeRebates;
    // }
    //
    // private static async Task FulfillOpen(List<TradeRebate> items,
    //     MetaTrade5DbContext ctx)
    // {
    //     var dealIds = items
    //         .Select(x => (ulong)x.DealId)
    //         .ToList();
    //
    //     var deals = await ctx.Mt5Deals
    //         .Where(x => dealIds.Contains(x.Deal))
    //         .Select(x => new { x.Deal, x.PositionId })
    //         .ToListAsync();
    //
    //     var positions = await ctx.Mt5Deals
    //         .Where(x => deals.Select(d => d.PositionId).Contains(x.PositionId))
    //         .Where(x => x.VolumeClosed == 0)
    //         .Select(x => new { x.PositionId, x.TimeMsc, x.Order, x.Price })
    //         .ToListAsync();
    //
    //     foreach (var item in items)
    //     {
    //         var deal = deals.FirstOrDefault(x => (long)x.Deal == item.DealId);
    //         if (deal == null) continue;
    //
    //         var pos = positions.FirstOrDefault(x => x.PositionId == deal.PositionId);
    //         if (pos == null) continue;
    //
    //         item.OpenedOn = DateTime.SpecifyKind(pos.TimeMsc, DateTimeKind.Utc);
    //         item.OpenPrice = pos.Price;
    //     }
    // }
}