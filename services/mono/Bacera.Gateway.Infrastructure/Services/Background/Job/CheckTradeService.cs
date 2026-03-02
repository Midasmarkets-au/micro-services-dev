using Bacera.Gateway.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bacera.Gateway.Services.Background;

public class CheckTradeService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CentralDbContext _centralDbContext;
    private readonly MyCache _myCache;

    private static TimeSpan ServiceDelaySeconds { get; } = TimeSpan.FromSeconds(10);

    public CheckTradeService(IServiceProvider serviceProvider
        , CentralDbContext centralDbContext
        , MyCache myCache
    )
    {
        _serviceProvider = serviceProvider;
        _centralDbContext = centralDbContext;
        _myCache = myCache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Your background service logic here...

            var tenantIds = await GetTenantIds();

            foreach (var tenantId in tenantIds)
            {
                // var mt4DbContext = await getMt4DbContext(tenantId);
                // var mt5DbContext = await getMt5DbContext(tenantId);
                // var hostingBackgroundService = new HostingBackgroundService(tenantId, _cache);
                //
                // var mt4Trades = await mt4DbContext.Mt4Trades
                //     .Select(x => new
                //     {
                //         x.Ticket,
                //         AccountNumber = x.Login,
                //         Platform = PlatformTypes.MetaTrader4,
                //     })
                //     .ToListAsync(cancellationToken: stoppingToken);
            }

            await Task.Delay(ServiceDelaySeconds, stoppingToken); // Wait for 10 seconds before the next iteration
        }
    }

    private List<long> _tenantIds = new();

    private async Task<List<long>> GetTenantIds()
    {
        if (_tenantIds.Any())
            return _tenantIds;

        var tenantIds = await _centralDbContext.Tenants
            .Select(x => x.Id)
            .ToListAsync();
        _tenantIds = tenantIds;
        return _tenantIds;
    }
}