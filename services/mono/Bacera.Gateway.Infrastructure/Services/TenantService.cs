using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

using M = Tenant;

public class TenantService(IMyCache myCache, MyDbContextPool pool, CentralDbContext centralDbContext) : ITenantService
{
    private const int CacheExpiresInMin = 30;
    private readonly string _cacheKey = $"central_tenants_{Environment.GetEnvironmentVariable("DB_USERNAME")}";

    public async Task<M> GetAsync(long id)
    {
        List<M> tenants;
        try
        {
            tenants = await GetAllTenantsAsync();
        }
        catch (Exception e)
        {
            BcrLog.Slack($"error getting tenants: {e.Message}");
            // await ResetCacheAsync();
            tenants = await centralDbContext.Tenants.AsNoTracking().ToListAsync();
        }

        return tenants.FirstOrDefault(x => x.Id == id) ?? new M();
    }

    public Task<List<M>> GetAllTenantsAsync() => myCache.GetOrSetAsync(_cacheKey
        , () => centralDbContext.Tenants.ToListAsync()
        , TimeSpan.FromMinutes(CacheExpiresInMin)
    );

    public Task ResetCacheAsync() => myCache.KeyDeleteAsync(_cacheKey);
}