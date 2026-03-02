namespace Bacera.Gateway;

using M = Tenant;

public interface ITenantService
{
    Task<List<M>> GetAllTenantsAsync();

    Task ResetCacheAsync();
    Task<M> GetAsync(long id);
}