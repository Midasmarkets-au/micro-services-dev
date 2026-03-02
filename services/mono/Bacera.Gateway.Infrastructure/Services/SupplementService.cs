namespace Bacera.Gateway.Services;

public class SupplementService(TenantDbContext tenantCtx)
{
    public async Task<Supplement> CreateAsync(SupplementTypes type, long rowId, string data = "{}")
    {
        var item = Supplement.Build(type, rowId, data);
        tenantCtx.Supplements.Add(item);
        await tenantCtx.SaveChangesAsync();
        return item;
    }
}