using Bacera.Gateway.Connection;
using Bacera.Gateway.Services.Extension;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Services;

public class StartupService(IServiceProvider provider)
{
    public async Task RunAsync()
    {
        await Task.Delay(0);
        try
        {
            using var outerScope = provider.CreateScope();
            var centralCtx = outerScope.ServiceProvider.GetRequiredService<CentralDbContext>();
            var tenantIds = await centralCtx.Tenants.Select(x => x.Id).ToListAsync();
            var tasks = tenantIds.Select(async tenantId =>
            {
                using var scope = provider.CreateTenantScope(tenantId);
                var con = scope.ServiceProvider.GetRequiredService<TenantDbConnection>();
                await con.ExecuteAsync("""
                                       update core."_Configuration"
                                       set "Category" = 'Account'
                                       where "Category" = 'account';

                                       update core."_Configuration"
                                       set "Category" = 'Party'
                                       where "Category" = 'party';

                                       update core."_Configuration"
                                       set "Category" = 'Public'
                                       where "Category" = 'public';
                                       """);
            });

            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            BcrLog.Slack($"Error_DoingStartupService, error: {e.Message}");
        }
    }
}