using Bacera.Gateway.Legacy;
using Bacera.Gateway.Services;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Console.Migrate.Tests;

public class Initial
{
    private readonly ILoggerFactory _loggerFactory;

    protected readonly AuthDbContext AuthDbCtx;
    protected readonly TenantDbContext TenantCtx;
    protected readonly LegacyDbContext LegacyCtx;
    protected readonly UserService UserSvc;


    protected Initial()
    {
        const int tenantId = 1;

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        IConfiguration config = builder.Build();

        _loggerFactory = LoggerFactory.Create(options =>
        {
            options
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        var startup = new Startup(config, _loggerFactory.CreateLogger<Startup>());

        // initialize central context
        var centralDbContext =
            new CentralDbContext(startup.GetCentralDbContextOption(startup.GetCentralDbConnectString()));

        var tenant = centralDbContext.Tenants.FirstOrDefault(x => x.Id == tenantId);
        if (tenant == null) throw new Exception("Tenant not found");

        // initialize context
        TenantCtx = new TenantDbContext(startup.GetTenantDbContextOption(tenant.DatabaseName));
        AuthDbCtx = new AuthDbContext(
            startup.GetAuthDbContextOption(startup.GetTenantDbConnectString(tenant.DatabaseName)));
        LegacyCtx = new LegacyDbContext(startup.GetLegacyDbContextOption(startup.GetLegacyDbConnectString()));
        var opts = Options.Create(new MemoryDistributedCacheOptions());
        IDistributedCache cache = new MemoryDistributedCache(opts);
        var tenantResolver = new TenancyResolver();
        tenantResolver.SetTenant(new Tenant { Id = 1 });
        UserSvc = new UserService(TenantCtx, AuthDbCtx, cache, tenantResolver, _loggerFactory);
    }
}