using Bacera.Gateway.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Services.Extension;

public static class ServiceProviderExtension
{
    public static IServiceScope CreateTenantScope(this IServiceProvider serviceProvider, long tenantId)
    {
        var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenantId);
        return scope;
    }

    public static MyDbContextPool GetDbPool(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<MyDbContextPool>();

    public static TenantDbContext GetTenantDbContext(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<TenantDbContext>();

    public static IMyCache GetRedisCache(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IMyCache>();
}