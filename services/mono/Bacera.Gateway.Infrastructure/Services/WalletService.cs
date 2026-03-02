using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

public class WalletService(TenantDbContext tenantCtx, IMyCache cache, ITenantGetter getter)
{
    private readonly long _tenantId = getter.GetTenantId();
    public Task<bool> WalletExistByIdAsync(long id, long? partyId = null)
        => QueryById(id)
            .Where(x => partyId == null || x.PartyId == partyId)
            .AnyAsync();

    public async Task<bool> IsWalletBelongToPartyAsync(long partyId, long walletId)
    {
        var hKey = CacheKeys.GetPartyIdByWalletIdHashKey(_tenantId);
        var value = await cache.HGetStringAsync(hKey, walletId.ToString());
        if (value != null) return long.Parse(value) == partyId;

        var partyIdFromDb = await tenantCtx.Wallets.AnyAsync(x => x.PartyId == partyId && x.Id == walletId);
        if (!partyIdFromDb) return false;

        await cache.HSetStringAsync(hKey, walletId.ToString(), partyId.ToString(), TimeSpan.FromDays(7));
        return true;
    }

    public async Task<long> GetPartyIdByWalletIdAsync(long walletId, bool fromDb = false)
    {
        if (fromDb) return await QueryById(walletId).Select(x => x.PartyId).SingleOrDefaultAsync();

        var hKey = CacheKeys.GetPartyIdByWalletIdHashKey(_tenantId);
        var value = await cache.HGetStringAsync(hKey, walletId.ToString());
        if (value != null) return long.Parse(value);

        var partyId = await QueryById(walletId).Select(x => x.PartyId).SingleOrDefaultAsync();
        if (partyId == 0) return 0;

        await cache.HSetStringAsync(hKey, walletId.ToString(), partyId.ToString(), TimeSpan.FromDays(7));
        return partyId;
    }

    public async Task<List<Wallet.ClientPageModel>> QueryForClientAsync(Wallet.ClientCriteria criteria)
    {
        var items = await tenantCtx.Wallets
            .PagedFilterBy(criteria)
            .ToClientPageModel()
            .ToListAsync();
        return items;
    }

    private IQueryable<Wallet> QueryById(long id) => tenantCtx.Wallets.Where(x => x.Id == id);
}