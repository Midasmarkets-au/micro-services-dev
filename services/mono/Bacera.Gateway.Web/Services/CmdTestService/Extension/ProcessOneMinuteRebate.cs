using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.Report.Models;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task ReleaseFromCsvFile()
    {
        const string path = "/Users/yixuan/Downloads/June-1-min-rebatesV1.0.csv";
        var file = new FileStream(path, FileMode.Open);
        using var parser = new TextFieldParser(file);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");

        var rebateIds = new List<long>();
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            var idString = fields?[0];
            if (idString == null || !long.TryParse(idString, out var id))
                continue;
            rebateIds.Add(id);
        }

        using var bar = CreateBar(rebateIds.Count, "Releasing Rebates");

        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
//         var con = _serviceProvider.GetDbPool().CreateTenantDbConnection(10000);
//         await con.ExecuteAsync($"""
//                                 update core."_Matter" set "StateId" = 510 where "Id" in ({string.Join(',', rebateIds)}) and "StateId" = 590;
//                                 """);
        const int size = 30;
        var page = 0;
        while (page * size < rebateIds.Count)
        {
            var ids = rebateIds.Skip(page * size).Take(size).ToList();
            var items = await ctx.Rebates
                .Where(x => ids.Contains(x.Id))
                .Include(x => x.IdNavigation)
                .ToListAsync();

            foreach (var item in items)
            {
                bar.Tick();
                if (item.IdNavigation.StateId != (int)StateTypes.RebateTradeClosedLessThanOneMinute)
                    continue;

                var hasReleased = await ctx.WalletTransactions
                    .AnyAsync(x => x.MatterId == item.Id && x.Amount == item.Amount);

                if (hasReleased)
                    continue;

                item.IdNavigation.StateId = (int)StateTypes.RebateOnHold;

                await ctx.SaveChangesAsync();
            }

            page++;
        }

//         var con = _serviceProvider.GetDbPool().CreateTenantDbConnection(10000);
//         await con.ExecuteAsync($"""
//                                 update core."_Matter" set "StateId" = 510 where "Id" in ({string.Join(',', rebateIds)}) and "StateId" = 590;
//                                 """);
    }

    private async Task CheckReleased()
    {
        var tickets = await GetTicketsFromFile();
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var dd = await ctx.TradeRebates
            .Where(x => tickets.Contains(x.Ticket))
            .SelectMany(x => x.Rebates)
            .Where(x => x.IdNavigation.StateId != (int)StateTypes.RebateCompleted)
            .ToListAsync();

        var ids = dd.Select(x => x.Id).ToList();
        Console.WriteLine($"{string.Join(',', ids)}");
    }

    private async Task ProcessOneMinRebate()
    {
        var tickets = await GetTicketsFromFile();
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var connection = scope.ServiceProvider.GetRequiredService<TenantDbConnection>();

        await connection.ExecuteAsync(
            $"""
             update trd.trade_rebate_k8s
             set status = {(int)TradeRebateStatusTypes.PendingResend}
             where id in (select tr.id
                            from trd.trade_rebate_k8s tr
                                     left join trd.rebate_k8s r on r.trade_rebate_id = tr.id
                            where tr.status = {(int)TradeRebateStatusTypes.SkippedWithOpenCloseTimeLessThanOneMinute}
                              and ticket in ({string.Join(',', tickets)})
                              and r.id is null);
             """
        );

        // execute the following when rebates from above trades are generated!!!!!!!!
        await connection.ExecuteAsync(
            $"""
             update core.matter_k8s
             set state_id = {(int)StateTypes.RebateOnHold}
             where id in (select r.id
                            from trd.rebate_k8s r
                                     join core.matter_k8s m on r.id = m.id
                                     join trd.trade_rebate_k8s tr on tr.id = r.trade_rebate_id
                            where m.state_id <> {(int)StateTypes.RebateCompleted}
                              and r.trade_rebate_id is not null
                              and tr.ticket IN ({string.Join(',', tickets)}))
             """
        );
    }


    private async Task GenerateReportForOneMinRebate()
    {
        var tickets = await GetTicketsFromFile();
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var query = ctx.TradeRebates
            .Where(x => tickets.Contains(x.Ticket))
            .SelectMany(x => x.Rebates)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.RebateCompleted)
            .Where(x => x.IdNavigation.WalletTransactions.Any());

        var count = await query.CountAsync();
        Console.WriteLine($"Rebates Count in Total: {count}");
        var amountSum = await query.SumAsync(x => x.Amount);
        Console.WriteLine($"Rebates Amount in Total: {amountSum}");


        // return;

        var tempFilePath = "/Users/yixuanyu/Desktop/dfdfdfdfdfdfdfdfdf.csv";
        await using var writer = new StreamWriter(tempFilePath, false, new UTF8Encoding(true));
        await writer.WriteLineAsync(WalletTransactionRecord.Header());
        using var bar = CreateBar(count, "Generating Rebate Report");
        var page = 1;
        var pageSize = 300;
        while (true)
        {
            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletTransactionRecord
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    CurrencyId = (CurrencyTypes)x.CurrencyId,
                    PartyId = x.PartyId,
                    FundType = (FundTypes)x.FundType,
                    Source = x.TradeRebate!.Ticket,
                    Target = 0,
                    RebateTargetAccountUid = x.Account.Uid,
                    WalletId = x.IdNavigation.WalletTransactions.First().WalletId,
                    MatterType = MatterTypes.Rebate,
                    StateId = (StateTypes)x.IdNavigation.StateId,
                    CreatedOn = x.IdNavigation.PostedOn,
                    ReleasedOn = x.IdNavigation.StatedOn,
                }).ToListAsync();

            if (items.Count == 0) break;

            var partyIds = items.Select(x => x.PartyId).Distinct().ToList();
            var users = await _authDbContext.Users
                .Where(x => x.TenantId == 10000 && partyIds.Contains(x.PartyId))
                .ToEmailNameModel()
                .ToListAsync();

            foreach (var item in items)
            {
                var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
                if (user == null)
                    continue;

                item.ClientName = user.GuessNativeName();
            }

            foreach (var item in items)
            {
                bar.Tick();
                await writer.WriteLineAsync(item.ToCsv());
            }

            page++;
        }


        // var tempDirectory = Path.GetTempPath();
        // var tempFilePath = Path.Combine(tempDirectory, Path.GetRandomFileName() + ".csv");

        // save file to temp directory
    }


    private const string Key = "april_trade_closed_1_minV1_tickets";
    private const string RebatePath = "/Users/yixuanyu/Desktop/april_trade_closed_1_minV1.0.csv";

    private async Task<List<long>> GetTicketsFromFile()
    {
        var myCache = _serviceProvider.GetRequiredService<IMyCache>();
        var tickets = await myCache.GetOrSetAsync(Key, async () =>
        {
            await Task.Delay(0);
            var items = new HashSet<long>();
            using var parser = new TextFieldParser(RebatePath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var ticketString = fields?[0];
                if (ticketString == null || !long.TryParse(ticketString, out var ticket))
                    continue;
                items.Add(ticket);
            }

            return items.ToList();
        });
        return tickets;
    }
}