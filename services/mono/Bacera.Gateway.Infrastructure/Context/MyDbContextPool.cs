using System.Collections.Concurrent;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;

namespace Bacera.Gateway.Context;

public class MyDbContextPool : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    private readonly SemaphoreSlim _centralPoolSemaphore;
    private readonly ConcurrentBag<CentralDbContext> _centralCtxPool;
    
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _tenantPoolSemaphores;
    private readonly ConcurrentDictionary<long, ConcurrentBag<TenantDbContext>> _tenantCtxPools;

    private readonly ConcurrentDictionary<int, SemaphoreSlim> _centralMt4PoolSemaphores;
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _centralMt5PoolSemaphores;
    private readonly ConcurrentDictionary<int, ConcurrentBag<MetaTrade4DbContext>> _centralMt4CtxPools;
    private readonly ConcurrentDictionary<int, ConcurrentBag<MetaTrade5DbContext>> _centralMt5CtxPools;

    private readonly ConcurrentDictionary<MetaTrade4DbContext, int> _centralMt4KeyMap = new();
    private readonly ConcurrentDictionary<MetaTrade5DbContext, int> _centralMt5KeyMap = new();
    private readonly ConcurrentDictionary<TenantDbContext, long> _tenantKeyMap = new();

    private const int CentralPoolSize = 5;
    private const int TenantPoolSize = 30;

    private readonly Dictionary<long, string> _tenantConnectionStringCache = new();
    private readonly Dictionary<int, string> _mtConnectionStringCache = new();
    private readonly Dictionary<int, PlatformTypes> _platformCache = new();
    private readonly Dictionary<int, string> _serviceNameCache = new();
    
    

    public MyDbContextPool(IServiceProvider serviceProvider)
    {
        _centralCtxPool = [];
        _tenantCtxPools = new ConcurrentDictionary<long, ConcurrentBag<TenantDbContext>>();
        _centralMt4CtxPools = new ConcurrentDictionary<int, ConcurrentBag<MetaTrade4DbContext>>();
        _centralMt5CtxPools = new ConcurrentDictionary<int, ConcurrentBag<MetaTrade5DbContext>>();
        _centralPoolSemaphore = new SemaphoreSlim(CentralPoolSize, CentralPoolSize);
        _tenantPoolSemaphores = new ConcurrentDictionary<long, SemaphoreSlim>();
        _centralMt4PoolSemaphores = new ConcurrentDictionary<int, SemaphoreSlim>();
        _centralMt5PoolSemaphores = new ConcurrentDictionary<int, SemaphoreSlim>();
        _serviceProvider = serviceProvider;
        InitializeCentralDbContextPool();
    }

    ~MyDbContextPool()
    {
        Dispose(false);
    }

    public async Task<TenantDbContext> BorrowTenant(long tenantId, CancellationToken cancellationToken = default)
    {
        var dbPool = _tenantCtxPools.GetOrAdd(tenantId, new ConcurrentBag<TenantDbContext>());
        var tenantSemaphore =
            _tenantPoolSemaphores.GetOrAdd(tenantId, new SemaphoreSlim(TenantPoolSize, TenantPoolSize));

        await tenantSemaphore.WaitAsync(cancellationToken);

        var ctx = dbPool.TryTake(out var dbContext)
            ? dbContext
            : CreateTenantDbContext(tenantId);

        _tenantKeyMap.TryAdd(ctx, tenantId);
        return ctx;
    }

    public async Task<MetaTrade4DbContext> BorrowCentralMT4Async(int serviceId,
        CancellationToken cancellationToken = default)
    {
        var dbPool = _centralMt4CtxPools.GetOrAdd(serviceId, new ConcurrentBag<MetaTrade4DbContext>());
        var semaphore =
            _centralMt4PoolSemaphores.GetOrAdd(serviceId, new SemaphoreSlim(CentralPoolSize, CentralPoolSize));

        await semaphore.WaitAsync(cancellationToken);

        var ctx = dbPool.TryTake(out var dbContext)
            ? dbContext
            : CreateCentralMT4DbContextAsync(serviceId);

        _centralMt4KeyMap.TryAdd(ctx, serviceId);
        return ctx;
    }


    public void ReturnCentralMT4(MetaTrade4DbContext ctx)
    {
        ctx.ChangeTracker.Clear();
        var serviceId = _centralMt4KeyMap[ctx];
        var dbPool = _centralMt4CtxPools.GetOrAdd(serviceId, new ConcurrentBag<MetaTrade4DbContext>());
        dbPool.Add(ctx);
        var tenantSemaphore =
            _centralMt4PoolSemaphores.GetOrAdd(serviceId, new SemaphoreSlim(CentralPoolSize, CentralPoolSize));
        tenantSemaphore.Release();
    }

    public async Task<MetaTrade5DbContext> BorrowCentralMT5Async(int serviceId,
        CancellationToken cancellationToken = default)
    {
        var dbPool = _centralMt5CtxPools.GetOrAdd(serviceId, new ConcurrentBag<MetaTrade5DbContext>());
        var semaphore =
            _centralMt5PoolSemaphores.GetOrAdd(serviceId, new SemaphoreSlim(CentralPoolSize, CentralPoolSize));

        await semaphore.WaitAsync(cancellationToken);

        var ctx = dbPool.TryTake(out var dbContext)
            ? dbContext
            : CreateCentralMT5DbContextAsync(serviceId);

        _centralMt5KeyMap.TryAdd(ctx, serviceId);
        return ctx;
    }


    public void ReturnCentralMT5(MetaTrade5DbContext ctx)
    {
        ctx.ChangeTracker.Clear();
        var serviceId = _centralMt5KeyMap[ctx];
        var dbPool = _centralMt5CtxPools.GetOrAdd(serviceId, new ConcurrentBag<MetaTrade5DbContext>());
        dbPool.Add(ctx);
        var tenantSemaphore =
            _centralMt5PoolSemaphores.GetOrAdd(serviceId, new SemaphoreSlim(CentralPoolSize, CentralPoolSize));
        tenantSemaphore.Release();
    }


    public void ReturnTenant(long tenantId, TenantDbContext dbContext)
    {
        dbContext.ChangeTracker.Clear();
        var dbPool = _tenantCtxPools.GetOrAdd(tenantId, new ConcurrentBag<TenantDbContext>());
        dbPool.Add(dbContext);
        var tenantSemaphore =
            _tenantPoolSemaphores.GetOrAdd(tenantId, new SemaphoreSlim(TenantPoolSize, TenantPoolSize));
        tenantSemaphore.Release();
    }

    public void ReturnTenant(TenantDbContext dbContext)
    {
        dbContext.ChangeTracker.Clear();
        var tenantId = _tenantKeyMap[dbContext];
        var dbPool = _tenantCtxPools.GetOrAdd(tenantId, new ConcurrentBag<TenantDbContext>());
        dbPool.Add(dbContext);
        var tenantSemaphore =
            _tenantPoolSemaphores.GetOrAdd(tenantId, new SemaphoreSlim(TenantPoolSize, TenantPoolSize));
        tenantSemaphore.Release();
    }

    public async Task<CentralDbContext> BorrowCentralAsync(CancellationToken cancellationToken = default)
    {
        await _centralPoolSemaphore.WaitAsync(cancellationToken);

        return _centralCtxPool.TryTake(out var dbContext)
            ? dbContext
            : CreateNewCentralDbContext(); // Fallback in case of an error, should not happen in normal operation
    }

    public void ReturnCentral(CentralDbContext dbContext)
    {
        _centralCtxPool.Add(dbContext);
        _centralPoolSemaphore.Release();
    }


    private void InitializeCentralDbContextPool()
    {
        for (var i = 0; i < Math.Max(CentralPoolSize, 1); i++)
        {
            _centralCtxPool.Add(CreateNewCentralDbContext());
        }

        var centralCtx = _centralCtxPool.First();
        using var scope = _serviceProvider.CreateScope();
        var tenants = centralCtx.Tenants.ToList();
        var tenantDbOptions = scope.ServiceProvider.GetRequiredService<IOptions<TenantDatabaseOptions>>().Value;
        foreach (var tenant in tenants)
        {
            tenantDbOptions.SetDatabase(tenant.DatabaseName);
            var connection = tenantDbOptions.GetConnectionString();
            _tenantConnectionStringCache[tenant.Id] = connection;
        }

        var tradeServices = centralCtx.CentralTradeServices.ToBasicModel().ToList();

        var cache = _serviceProvider.GetRedisCache();
        var key = CacheKeys.GetCentralTradeServiceKey();
        cache.SetStringAsync(key, JsonConvert.SerializeObject(tradeServices));

        foreach (var tradeService in tradeServices)
        {
            var tradeServiceOptions = JsonConvert.DeserializeObject<TradeServiceOptions>(tradeService.Configuration)!;
            var connection = tradeServiceOptions.Database!.GetConnectionString();
            _mtConnectionStringCache[tradeService.Id] = connection;
            _platformCache[tradeService.Id] = (PlatformTypes)tradeService.Platform;
            _serviceNameCache[tradeService.Id] = tradeService.Name;
        }
    }

    private CentralDbContext CreateNewCentralDbContext()
    {
        using var scope = _serviceProvider.CreateScope();
        var centralDbOptions = scope.ServiceProvider.GetRequiredService<IOptions<CentralDatabaseOptions>>().Value;
        var centralDbCtxBuilder = new DbContextOptionsBuilder<CentralDbContext>();
        centralDbCtxBuilder.UseNpgsql(centralDbOptions.GetConnectionString());
        return new CentralDbContext(centralDbCtxBuilder.Options);
    }

    public string GetConnectionStringByTenantId(long tenantId) =>
        _tenantConnectionStringCache.GetValueOrDefault(tenantId, string.Empty);

    public int GetTenantCount() => _tenantConnectionStringCache.Count;

    public List<long> GetTenantIds() => _tenantConnectionStringCache.Keys.ToList();

    public TenantDbConnection CreateTenantDbConnection(long tenantId)
    {
        var connection = GetConnectionStringByTenantId(tenantId);
        return new TenantDbConnection(new NpgsqlConnection(connection));
    }

    public TenantDbContext CreateTenantDbContext(long tenantId, bool enableLogging = false)
    {
        var connection = GetConnectionStringByTenantId(tenantId);

        var builder = new DbContextOptionsBuilder<TenantDbContext>();
        if (enableLogging)
        {
            connection += ";Include Error Details=True";
            builder.LogTo(Console.WriteLine);
            builder.EnableSensitiveDataLogging();
        }

        builder.UseNpgsql(connection);
        return new TenantDbContext(builder.Options);
    }


    public MetaTrade4DbContext CreateCentralMT4DbContextAsync(int serviceId, bool enableLogging = false)
    {
        var connection = _mtConnectionStringCache[serviceId];
        var builder = new DbContextOptionsBuilder<MetaTrade4DbContext>();
        builder.UseMySql(connection, ServerVersion.Parse("5.7.38-mysql"));
        if (!enableLogging) return new MetaTrade4DbContext(builder.Options);

        builder.LogTo(Console.WriteLine);
        builder.EnableSensitiveDataLogging();
        return new MetaTrade4DbContext(builder.Options);
    }

    public MetaTrade5DbContext CreateCentralMT5DbContextAsync(int serviceId, bool enableLogging = false)
    {
        var connection = _mtConnectionStringCache[serviceId];
        var builder = new DbContextOptionsBuilder<MetaTrade5DbContext>();
        builder.UseMySql(connection, ServerVersion.Parse("5.7.38-mysql"));
        if (!enableLogging) return new MetaTrade5DbContext(builder.Options);

        builder.LogTo(Console.WriteLine);
        builder.EnableSensitiveDataLogging();
        return new MetaTrade5DbContext(builder.Options);
    }

    public PlatformTypes GetPlatformByServiceId(int serviceId) => _platformCache[serviceId];
    public string GetServiceNameByServiceId(int serviceId) => _serviceNameCache.GetValueOrDefault(serviceId, "");
    public Dictionary<int, string> GetServiceNameDict() => _serviceNameCache;
    public bool IsServiceExisted(int serviceId) => _platformCache.ContainsKey(serviceId);
    
    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (!disposing) return;
        _centralPoolSemaphore.Dispose();
        _tenantPoolSemaphores.Values.ForEach(x => x.Dispose());
        _centralCtxPool.ForEach(x => x.Dispose());
        _tenantCtxPools.Values.SelectMany(x => x).ForEach(x => x.Dispose());
        _centralMt4CtxPools.Values.SelectMany(x => x).ForEach(x => x.Dispose());
        _centralMt5CtxPools.Values.SelectMany(x => x).ForEach(x => x.Dispose());
        _centralMt4PoolSemaphores.Values.ForEach(x => x.Dispose());
        _centralMt5PoolSemaphores.Values.ForEach(x => x.Dispose());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public static class DbContextExtension
{
    public static bool IsConnectionValid(this DbContext ctx)
    {
        var connection = ctx.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connection))
            return false;
        
        try
        {
            ctx.Database.OpenConnection();
            ctx.Database.CloseConnection(); // CRITICAL: Close the connection to prevent leaks
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"Error_TenantDbContext_IsConnectionValid: {e.Message}, Connection:{connection}");
            return false;
        }
    }
}