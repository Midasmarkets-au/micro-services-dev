using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Services;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class CryptoJob(MyDbContextPool pool, IServiceProvider serviceProvider, IMyCache cache)
{
    [DisableConcurrentExecution(3600)]
    public async Task MonitorAsync()
    {
        foreach (var tenantId in pool.GetTenantIds())
        {
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var tenantCtx = scope.ServiceProvider.GetTenantDbContext();

            // Add this debug logging
            var connectionString = tenantCtx.Database.GetConnectionString();
            BcrLog.Slack($"CryptoJob: TenantId={tenantId}, ConnectionString={connectionString}");

            var cryptoSvc = scope.ServiceProvider.GetRequiredService<CryptoService>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var cryptos = await tenantCtx.Cryptos.ToListAsync();
            foreach (var crypto in cryptos)
            {
                List<long> matchedDepositIds;
                try
                {
                    matchedDepositIds = await cryptoSvc.TronProSyncTransactionAsync(crypto);
                }
                catch (Exception e)
                {
                    BcrLog.Slack($"MonitorAsyncError:{e.Message}");
                    return;
                }

                foreach (var depositId in matchedDepositIds)
                {
                    await mediator.Publish(new DepositCompletedEvent(depositId));
                }

                if (crypto.IsOnHold())
                {
                    var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();
                    var setting = await cfgSvc.GetAsync<Crypto.Setting>(nameof(Public), 0, ConfigKeys.CryptoSetting);
                    if (DateTime.UtcNow - crypto.UpdatedOn <= TimeSpan.FromMinutes(setting!.PayExpiredTimeInMinutes))
                        continue;

                    await cache.KeyDeleteAsync(CacheKeys.CryptoWalletKey(crypto.Address));
                    crypto.Release();
                    await tenantCtx.SaveChangesAsync();
                }

                await Task.Delay(300);
            }
        }
    }
}