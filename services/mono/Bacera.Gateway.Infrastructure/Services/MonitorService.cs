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
        var pattern = $"{prefixKey}:*";

        var allKeys = new List<string>();
        long cursor = 0;
        do
        {
            var scan = await db.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "100");
            var result = (RedisResult[])scan!;
            cursor = long.Parse((string)result[0]!);
            allKeys.AddRange((string[])result[1]!);
        } while (cursor != 0);

        var results = new List<object>();
        foreach (var key in allKeys)
        {
            var value = await db.StringGetAsync(key);
            if (value.HasValue) results.Add(value);
        }

        return results;
    }

    public async Task ClearOnlineAdminAsync()
    {
        var cache = provider.GetRequiredService<IMyCache>();
        var db = cache.GetDatabase();
        var prefixKey = CacheKeys.GetWsOnlineAdminHKey();
        var pattern = $"{prefixKey}:*";

        var allKeys = new List<string>();
        long cursor = 0;
        do
        {
            var scan = await db.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "100");
            var result = (RedisResult[])scan!;
            cursor = long.Parse((string)result[0]!);
            allKeys.AddRange((string[])result[1]!);
        } while (cursor != 0);

        foreach (var key in allKeys)
        {
            await db.KeyDeleteAsync(key);
        }
    }
}