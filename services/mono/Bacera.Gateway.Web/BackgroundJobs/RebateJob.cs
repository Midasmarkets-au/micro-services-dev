using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Bacera.Gateway.Web.BackgroundJobs;

public class RebateJob(
    IServiceProvider serviceProvider,
    CentralDbContext centralDbContext,
    MyDbContextPool pool,
    IMyCache cache)
    : IRebateJob
{
   

    [DisableConcurrentExecution(3600)]
    public async Task<(bool, string)> CalculateRebate()
    {
        const int size = 30;
        var tasks = serviceProvider.GetDbPool().GetTenantIds().Select(async tenantId =>
        {
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            if (true != await cfgSvc.GetRebateToggleSwitchAsync())
                return (true, $"Rebate calculation is disabled. tid:{tenantId}");

            var rebateSvc = scope.ServiceProvider.GetRequiredService<RebateService>();
            var tenantCtx = scope.ServiceProvider.GetTenantDbContext();

            var tradeRebateQuery = tenantCtx.TradeRebates
                .OrderBy(x => x.Id)
                .Where(x => x.Status == (short)TradeRebateStatusTypes.PendingResend ||
                            x.Status == (short)TradeRebateStatusTypes.Created);

            if (!await tradeRebateQuery.AnyAsync()) return (true, $"No trade rebate found. tid:{tenantId}");

            var minId = await tradeRebateQuery.MinAsync(x => x.Id);
            var maxId = await tradeRebateQuery.MaxAsync(x => x.Id);

            var lockKey = DistributedLockKeys.GetCalculateRebateKey(tenantId);
            var lockValue = Guid.NewGuid().ToString();
            if (!cache.TryGetDistributedLock(lockKey, lockValue, TimeSpan.FromMinutes(70)))
                return (false, $"Failed to acquire lock. tid:{tenantId}");

            var page = 1;
            try
            {
                while (true)
                {
                    var ids = await tenantCtx.TradeRebates
                        .Where(x => x.Id >= minId && x.Id <= maxId)
                        .OrderBy(x => x.Id)
                        .Skip((page - 1) * size).Take(size)
                        .Select(x => x.Id)
                        .ToListAsync();

                    foreach (var id in ids)
                    {
                        await rebateSvc.GenerateRebatesByTradeRebateId(id, true);
                    }

                    if (ids.Count < size) break;
                    ++page;
                }
            }
            catch (Exception e)
            {
                BcrLog.Slack($"CalculateRebateV2_tid:{tenantId}_Error:{e}");
            }
            finally
            {
                cache.ReleaseDistributedLock(lockKey, lockValue);
            }

            return (true, $"Rebate calculation completed. tid:{tenantId}");
        });

        var results = await Task.WhenAll(tasks);
        return results.Aggregate((true, ""), (acc, x) => (acc.Item1 && x.Item1, $"{acc.Item2} {x.Item2}"));
    }

    [DisableConcurrentExecution(3600)]
    public async Task ReleaseRebateAsync()
    {
        var key = CacheKeys.GetReleaseDisabledKey();
        if (await cache.GetStringAsync(key) != null)
            return;

        await Task.WhenAll(serviceProvider.GetDbPool().GetTenantIds().Select(async tenantId =>
        {
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
            if (true != await cfgSvc.GetRebateToggleSwitchAsync())
                return;

            await using var ctx = pool.CreateTenantDbContext(tenantId);
            var acctSvc = scope.ServiceProvider.GetRequiredService<AcctService>();

            var ids = await ctx.Rebates
                .Where(x => x.IdNavigation.StateId == (int)StateTypes.RebateReleased || x.IdNavigation.StateId == (int)StateTypes.RebateOnHold)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var id in ids)
            {
                await acctSvc.RebateCompleteAsync(id, 1, "Release By System");
            }
        }));
    }
}

// [DisableConcurrentExecution(3600)]
// public async Task<Dictionary<long, int>> GenerateTradeRebate()
// {
//     var result = new Dictionary<long, int>();
//     var tenants = await centralDbContext.Tenants
//         .Select(x => new { x.Id })
//         .ToListAsync();
//
//     foreach (var tenant in tenants)
//     {
//         using var scope = serviceProvider.CreateScope();
//         var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
//         tenancyResolver.SetTenantId(tenant.Id);
//
//         // check if rebate is enabled
//         var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
//         if (true != await cfgSvc.GetRebateToggleSwitchAsync())
//             continue;
//
//         var tradingService = scope.ServiceProvider.GetRequiredService<TradingService>();
//         var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
//         var logger = scope.ServiceProvider.GetRequiredService<ILogger<RebateJob>>();
//         var tradeServices = await ctx.TradeServices
//             .Where(x => x.Platform == (int)PlatformTypes.MetaTrader4
//                         || x.Platform == (int)PlatformTypes.MetaTrader5)
//             .ToListAsync();
//         var count = 0;
//         foreach (var svc in tradeServices)
//         {
//             logger.LogInformation($"GenerateTradeRebateAsyncCheckTradeService: Processing {svc.Name} Id:{svc.Id}.");
//             while (true)
//             {
//                 var updatedCount = await tradingService.GenerateTradeRebateAsync(svc);
//                 count += updatedCount;
//                 if (updatedCount == 0)
//                     break;
//             }
//         }
//
//         result.Add(tenant.Id, count);
//     }
//
//     return result;
// }