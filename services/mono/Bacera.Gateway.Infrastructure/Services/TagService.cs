using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

public class TagService(TenantDbContext tenantCtx, ITenantGetter getter, IMyCache cache)
{
    private readonly long _tenantId = getter.GetTenantId();

    public Task<List<Tag.BasicModel>> GetAllTags()
        => cache.GetOrSetAsync(CacheKeys.GetAllTagKey(_tenantId)
            , () => tenantCtx.Tags.ToBasicModels().ToListAsync()
            , TimeSpan.FromHours(1));

    public Task<bool> AddTagForTypeAsync(string type, string name, long rowId)
        => type switch
        {
            "account" => AddAccountTagAsync(rowId, name),
            "party" => AddPartyTagAsync(rowId, name),
            _ => Task.FromResult(false)
        };

    public Task<bool> RemoveTagForTypeAsync(string type, string name, long rowId)
        => type switch
        {
            "account" => RemoveAccountTagAsync(rowId, name),
            "party" => RemovePartyTagAsync(rowId, name),
            _ => Task.FromResult(false)
        };
    
    public async Task<bool> AddPartyTagAsync(long partyId, string name)
    {
        var tag = await GetTagAsync(name);
        if (tag == null) return false;

        var party = await tenantCtx.Parties
            .Include(x => x.Tags.Where(y => y.Name == name))
            .FirstOrDefaultAsync(x => x.Id == partyId);
        if (party == null) return false;
        if (party.Tags.Any(x => x.Id == tag.Id)) return true;

        party.Tags.Add(tag);
        party.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Parties.Update(party);
        return await tenantCtx.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemovePartyTagAsync(long partyId, string name)
    {
        var tag = await GetTagAsync(name);
        if (tag == null) return false;

        var party = await tenantCtx.Parties
            .Include(x => x.Tags.Where(y => y.Name == name))
            .FirstOrDefaultAsync(x => x.Id == partyId);
        if (party == null) return false;
        if (party.Tags.All(x => x.Id != tag.Id)) return true;

        party.Tags.Remove(tag);
        party.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Parties.Update(party);
        return await tenantCtx.SaveChangesAsync() > 0;
    }

    public async Task<bool> AddAccountTagAsync(long accountId, string name)
    {
        var tag = await GetTagAsync(name);
        if (tag == null) return false;

        var account = await tenantCtx.Accounts
            .Include(x => x.Tags.Where(y => y.Name == name))
            .FirstOrDefaultAsync(x => x.Id == accountId);
        if (account == null) return false;
        if (account.Tags.Any(x => x.Id == tag.Id)) return true;

        account.Tags.Add(tag);
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        return await tenantCtx.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveAccountTagAsync(long accountId, string name)
    {
        var tag = await GetTagAsync(name);
        if (tag == null) return false;

        var account = await tenantCtx.Accounts
            .Include(x => x.Tags.Where(y => y.Name == name))
            .FirstOrDefaultAsync(x => x.Id == accountId);
        if (account == null) return false;
        if (account.Tags.All(x => x.Id != tag.Id)) return true;

        account.Tags.Remove(tag);
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        return await tenantCtx.SaveChangesAsync() > 0;
    }

    private async Task<Tag?> GetTagAsync(string name)
    {
        return await tenantCtx.Tags.FirstOrDefaultAsync(x => x.Name == name);
    }
}