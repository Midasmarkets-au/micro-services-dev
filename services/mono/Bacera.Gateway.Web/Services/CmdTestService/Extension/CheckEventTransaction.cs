using Bacera.Gateway.Connection;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    public async Task CheckEventTransactionAsync()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var eventSvc = scope.ServiceProvider.GetRequiredService<EventService>();

        // var result = await eventSvc.ProcessTradeSourceAsync(1, 633799);
        var result = await eventSvc.ProcessTradeRewardForParentAsync(1, 633799);
    }

    public async Task ManualEnqueueEventTransactionSource()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10004);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        
        
        var items = await tenantCtx.TradeRebates
            .Where(x => x.Id >= 1030578 && x.Id <= 1046938)
            .Where(x => x.Reason != 1 && x.Reason != 2)
            .Where(x => x.ClosedOn - x.OpenedOn > TimeSpan.FromMinutes(1))
            .Select(x => EventShopPointTransaction.MQSource.Build(EventShopPointTransactionSourceTypes.Trade, x.Id, 10004))
            .ToListAsync();
        using var bar = CreateBar(items.Count);

        var mqService = _serviceProvider.GetRequiredService<IMessageQueueService>();

        // var threadCount = 10;
        // var batchSize = (int)Math.Ceiling((double)items.Count / threadCount);
        // var tasks = new List<Task>();
        //
        // for (var i = 0; i < threadCount; i++)
        // {
        //     var batch = items.Skip(i * batchSize).Take(batchSize).ToList();
        //     tasks.Add(Task.Run(async () =>
        //     {
        //         foreach (var item in batch)
        //         {
        //             bar.Tick();
        //             var message = item.ToString();
        //             await mqService.SendAsync(message, "BCREventTrade.fifo", "BCREventTrade.fifo");
        //         }
        //     }));
        // }

        foreach (var item in items)
        {
            bar.Tick();
            var message = item.ToString();
            await mqService.SendAsync(message, "BCREventTrade.fifo", "BCREventTrade.fifo");
        }
        //
        // var json = """
        //             [
        //                 {
        //                    "SourceType": 1,
        //                    "RowId": 448191,
        //                    "TenantId": 10000
        //                 },
        //            ]
        //            """;

        // var json = """
        //            ['740885', '740883', '740326', '738666', '737999', '737775', '734957', '734955', '734943', '734935', '734868', '734867', '734301', '734202', '734201', '734071', '732488', '732472', '732101', '731405', '731171', '731143', '731100', '731222', '731073', '728419', '728410', '728406', '728226', '727779', '726934', '726572', '725663', '725204', '725203', '716666', '714047', '713432', '710521', '710247']
        //            """;

        // var json = """
        //            '724297', '724295', '714133', '713981', '713980'
        //            """;
        // // var items = JsonConvert.DeserializeObject<List<EventShopPointTransaction.MQSource>>(json)!;
        // var trades = json.Split(',').Select(x => long.Parse(x.Trim().Trim('\''))).ToList();
        // // var scope = CreateTenantScopeByTenantIdAsync(10000);
        // // var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        // // var con = scope.ServiceProvider.GetRequiredService<TenantDbConnection>();
        //
        // // "Id" IN ('724297', '724295', '714133', '713981', '713980')
        //
        // // var trades = await ctx.TradeRebates
        // //     .Where(x => x.AccountId == 54016 && x.Id != 633731)
        // //     .ToListAsync();
        // var items = trades.Select(x => new EventShopPointTransaction.MQSource
        // {
        //     SourceType = EventShopPointTransactionSourceTypes.Trade,
        //     RowId = x,
        //     TenantId = 10000
        // }).ToList();

        // var mqService = _serviceProvider.GetRequiredService<IMessageQueueService>();
        // foreach (var item in items)
        // {
        //     var message = item.ToString();
        //     await mqService.SendAsync(message, "BCREventTrade.fifo", "BCREventTrade.fifo");
        // }
    }

    public async Task ProcessDuplicateEventTrade()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var con = scope.ServiceProvider.GetRequiredService<TenantDbConnection>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var items = await con.ToListAsync<(long Ticket, int Count)>
        (
            $"""
             select ept."SourceContent"::jsonb ->> 'Ticket' as "Ticket", count(*) as "Count"
             from event."_EventShopPointTransaction" ept
                      join trd."_TradeRebate" tr on tr."Ticket" = cast(ept."SourceContent"::jsonb ->> 'Ticket' as bigint)
             --where ept."EventPartyId" = 48
             group by ept."SourceContent"::jsonb ->> 'Ticket'
             having count(*) = 2
             order by count(*) desc;
             """
        );

        using var bar = CreateBar(items.Count);

        foreach (var (ticket, count) in items)
        {
            bar.Tick($"Processing ticket {ticket} with {count} duplicates");
            var transactions = await ctx.EventShopPointTransactions
                .FromSqlInterpolated($"""
                                      select * from event."_EventShopPointTransaction"
                                      where "SourceContent"::jsonb ->> 'Ticket' = {ticket.ToString()}
                                      """
                )
                .OrderBy(x => x.CreatedOn)
                .ToListAsync();

            // remove duplicate point
            var tt = transactions.GroupBy(x => x.Point)
                .Select(x => x.Last())
                .ToList();

            var eventPartyPoint = await ctx.EventShopPoints
                .Where(x => x.EventPartyId == transactions.First().EventPartyId)
                .SingleAsync();

            var summed = tt.Sum(t => t.Point);
            eventPartyPoint.Point -= summed;
            eventPartyPoint.TotalPoint -= summed;
            ctx.EventShopPoints.Update(eventPartyPoint);
            ctx.EventShopPointTransactions.RemoveRange(transactions);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task ProcessAssignAccountId()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var con = scope.ServiceProvider.GetRequiredService<TenantDbConnection>();
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var date = Utils.ParseToUTC("2024-05-18 00:24:45 +00:00");

        var items = await con.ToListAsync<(long Ticket, int Count)>
        (
            $"""
             select ept."SourceContent"::jsonb ->> 'Ticket' as "Ticket", count(*) as "Count"
             from event."_EventShopPointTransaction" ept
                      join trd."_TradeRebate" tr on tr."Ticket" = cast(ept."SourceContent"::jsonb ->> 'Ticket' as bigint)
             --where ept."EventPartyId" = 48
             group by ept."SourceContent"::jsonb ->> 'Ticket'
             and ept."CreatedOn" <= {date}
             having count(*) = 1
             order by count(*) desc;
             """
        );

        var tickets = items.Select(x => x.Ticket).ToList();
        var tradeRebates = await ctx.TradeRebates
            .Where(x => tickets.Contains(x.Ticket))
            .Select(x => new { x.Id, x.Ticket, x.AccountId })
            .ToListAsync();

        var accs = await ctx.Accounts
            .Where(x => tradeRebates.Select(y => y.AccountId).Contains(x.Id))
            .Select(x => new { x.Id, x.AgentAccountId })
            .ToListAsync();

        // ensure all tickets have trade rebate
        if (tradeRebates.Count != items.Count)
        {
            throw new Exception("Trade rebate not found for all tickets");
        }

        if (accs.Any(x => x.AgentAccountId == null))
        {
            throw new Exception("Agent account not found for all accounts");
        }

        using var bar = CreateBar(items.Count);

//         var transactionsAll = await ctx.EventShopPointTransactions
//             .FromSqlInterpolated($"""
//                                   select * from event."_EventShopPointTransaction"
//                                   where "SourceContent"::jsonb ->> 'Ticket' in ({tickets.Select(x => x.ToString())})
//                                   """
//             )
//             .OrderBy(x => x.Point)
//             .ToListAsync();

        var ticketsStr = string.Join(",", tickets.Select(x => $"'{x}'"));
        var transactionsAll = await con.ToListAsync<EventShopPointTransaction>
        ($"""
          select * from event."_EventShopPointTransaction"
          where "SourceContent"::jsonb ->> 'Ticket' in ({ticketsStr})
          """
        );

        foreach (var (ticket, count) in items)
        {
            bar.Tick($"Processing ticket {ticket} with {count} duplicates");
            var transactions = transactionsAll
                .Where(x => x.SourceContent.Contains("Ticket") && x.SourceContent.Contains(ticket.ToString())).ToList();

            var tradeRebate = tradeRebates.Single(x => x.Ticket == ticket);
            var account = accs.Single(x => x.Id == tradeRebate.AccountId);

            var smallerOne = transactions.First();
            smallerOne.AccountId = account.AgentAccountId;
            smallerOne.SourceId = tradeRebate.Id;

            var biggerOne = transactions.Last();
            biggerOne.AccountId = account.Id;
            biggerOne.SourceId = tradeRebate.Id;
            ctx.EventShopPointTransactions.UpdateRange(transactions);
            await ctx.SaveChangesAsync();
        }
    }
}