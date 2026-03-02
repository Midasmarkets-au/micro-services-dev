using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Context;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task UpdateTradeAccountStatus()
    {
        using var scope = CreateTenantScopeByTenantIdAsync(10000);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var generalJob = scope.ServiceProvider.GetRequiredService<IGeneralJob>();


        var ids = await tenantCtx.Accounts.Where(x => x.AccountNumber != 0)
            .Select(x => x.Id).ToListAsync();

        using var bar = CreateBar(ids.Count);

        foreach (var id in ids)
        {
            bar.Tick();
            await generalJob.TryUpdateTradeAccountStatus(1, id);
            await generalJob.TryUpdateTradeAccountStatus(10000, id);
        }
    }

    private async Task CheckMissedTrade()
    {
        const long tenantId = 1;
        const int serviceId = 20;
        var endTime = Utils.ParseToUTC("2024-03-27 19:20:00.000000 +00:00");
        var startTime = Utils.ParseToUTC("2024-03-26 23:00:00.000000 +00:00");
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var tenantDbCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var myDbContextPool = scope.ServiceProvider.GetRequiredService<MyDbContextPool>();
        await using var mt4DbCtx = myDbContextPool.CreateCentralMT4DbContextAsync(serviceId);
        var centralTrades = await _centralDbContext.MetaTrades
            .Where(x => x.TenantId == tenantId && x.ServiceId == serviceId)
            .Where(x => x.CloseAt >= startTime && x.CloseAt <= endTime)
            .Select(x => new { x.Ticket, x.CloseAt })
            .OrderBy(x => x.Ticket)
            .ToListAsync();

        var tenantTrades = await mt4DbCtx.Mt4Trades
            .Where(x => x.Cmd == 0 || x.Cmd == 1)
            .Where(x => x.CloseTime >= startTime && x.CloseTime <= endTime)
            .OrderBy(x => x.CloseTime)
            .ToMetaTrade(0, serviceId)
            .ToListAsync();
        var accountNumbers = tenantTrades.Select(x => x.AccountNumber).Distinct().ToList();
        var accountNumberInTenants = await tenantDbCtx.Accounts
            .Where(x => x.ServiceId == serviceId)
            .Where(x => accountNumbers.Contains(x.AccountNumber))
            .Select(x => x.AccountNumber)
            .ToListAsync();
        tenantTrades = tenantTrades.Where(x => accountNumberInTenants.Contains(x.AccountNumber)).ToList();
        if (centralTrades.Count == tenantTrades.Count)
        {
            _logger.LogInformation("CheckMissedTrade: No missed trade.");
            return;
        }

        if (centralTrades.Count < tenantTrades.Count)
        {
            _logger.LogWarning("CheckMissedTrade: Central trades count less than tenant trades count.");
            var missedTrades = tenantTrades.Where(x => centralTrades.All(y => y.Ticket != x.Ticket)).ToList();
            var ticketString = missedTrades.Select(x => x.Ticket.ToString())
                .Aggregate((x, y) => $"{x},{y}");
            _logger.LogWarning("CheckMissedTrade: Missed trades: {tickets}", ticketString);

            var tasks = missedTrades
                .Select(JsonConvert.SerializeObject)
                .Select(x => _mqService.SendAsync(x, "BCRTrade"));
            await Task.WhenAll(tasks);
            return;
        }

        if (centralTrades.Count > tenantTrades.Count)
        {
            _logger.LogWarning("CheckMissedTrade: Tenant trades count less than central trades count.");
            var missedTrades = centralTrades.Where(x => tenantTrades.All(y => y.Ticket != x.Ticket)).ToList();
            var tickets = missedTrades.Select(x => x.Ticket.ToString())
                .Aggregate((x, y) => $"{x},{y}");
            _logger.LogWarning("CheckMissedTrade: Missed trades: {tickets}", tickets);
        }
    }
}
