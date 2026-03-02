using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Bacera.Gateway.Services;

public class MonitorService(IServiceProvider provider)
{
    public async Task UpdateUserRealTimeAsync(long tenantId, long partyId)
    {
        var cache = provider.GetRequiredService<IMyCache>();
        var tidPidToEmailKey = $"mybcr_MonitorService_UpdateUserRealTimeAsync_{tenantId}_{partyId}";
        var email = await cache.GetStringAsync(tidPidToEmailKey);
        if (email == null)
        {
            // await using var ctx = provider.GetDbPool().CreateTenantDbContext(tenantId);
            var pool = provider.GetDbPool();
            var ctx = await pool.BorrowTenant(tenantId);
            try
            {
                email = await ctx.Parties
                    .Where(x => x.Id == partyId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
                if (email == null) return;

                await cache.SetStringAsync(tidPidToEmailKey, email, TimeSpan.FromDays(1));
            }
            catch (Exception)
            {
                // _logger.LogError(e, "UpdateUserRealTimeAsync");
            }
            finally
            {
                pool.ReturnTenant(ctx);
            }
        }

        var hkey = CacheKeys.GetWsOnlineAdminHKey();
        var key = $"{hkey}:{partyId}";
        await cache.SetStringAsync(key, $"{email}_{partyId}_{tenantId}_{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}",
            TimeSpan.FromMinutes(5));
    }

    public async Task<List<object>> GetOnlineAdminAsync()
    {
        var cache = provider.GetRequiredService<IMyCache>();
        var db = cache.GetDatabase();
        var prefixKey = CacheKeys.GetWsOnlineAdminHKey();

        var results = new List<object>();
        var server = cache.GetServer();
        var keys = server.Keys(pattern: $"{prefixKey}:*", pageSize: 100);

        foreach (var key in keys)
        {
            var value = await db.StringGetAsync(key);
            results.Add(value);
        }

        return results;
    }

    public async Task ClearOnlineAdminAsync()
    {
        var cache = provider.GetRequiredService<IMyCache>();
        var db = cache.GetDatabase();
        var prefixKey = CacheKeys.GetWsOnlineAdminHKey();

        var server = cache.GetServer();
        var keys = server.Keys(pattern: $"{prefixKey}:*", pageSize: 100);
        foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }
    }
}