using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor.Amazon.Options;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Bacera.Gateway.Vendor.Amazon;
using Bacera.Gateway.Web.BackgroundJobs;
using StackExchange.Redis;

namespace Bacera.Gateway.Web.Tests;

public class Startup
{
    protected readonly Faker Faker;
    protected readonly ServiceProvider AppServiceProvider;
    protected readonly TradingService TradingService;
    protected readonly TenancyResolver TenancyResolver;
    protected readonly UserManager<User> UserManager;
    protected readonly TenantDbContext TenantDbContext;
    protected readonly CentralDbContext CentralDbContext;
    protected readonly ApplicationService ApplicationService;
    protected readonly ConfigurationService ConfigurationService;
    protected readonly AuthDbContext AuthDbContext;
    protected readonly IAccountingService AccountingService;
    protected readonly IApplicationTokenService TokenService;

    protected const int Mt4Port = 4578;
    protected const string Mt4Host = "13.57.233.166";

    /**
     * Migrate:
     * dotnet ef database update -c AuthDbContext --connection  "Host=localhost;Database=portal_central;Username=postgres;Password=dev"
     * dotnet ef database update -c CentralDbContext --connection  "Host=localhost;Database=portal_central;Username=postgres;Password=dev"
     */
    protected Startup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<TenancyResolver>();
        serviceCollection.AddScoped(typeof(ITenantGetter), svc => svc.GetRequiredService<TenancyResolver>());
        serviceCollection.AddScoped(typeof(ITenantSetter), svc => svc.GetRequiredService<TenancyResolver>());


        serviceCollection.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        serviceCollection.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "portal_unit_test:_";
        });

        serviceCollection.AddDbContext<CentralDbContext>(options
            => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        serviceCollection.AddDbContext<AuthDbContext>((services, options) =>
        {
            var tenant = services.GetRequiredService<ITenantGetter>().Tenant;
            options.UseNpgsql(configuration.GetConnectionString("TenantConnectionTemplate")!
                .Replace("{{DATABASE}}", tenant.DatabaseName));
        });

        serviceCollection.AddDbContext<TenantDbContext>((services, options) =>
        {
            var tenant = services.GetRequiredService<ITenantGetter>().Tenant;
            var connectionString = configuration.GetConnectionString("TenantConnectionTemplate")!
                .Replace("{{DATABASE}}", tenant.DatabaseName);
            options.UseNpgsql(connectionString);
        });

        serviceCollection.AddIdentity<User, Auth.ApplicationRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddUserManager<UserManager<User>>();

        serviceCollection.AddLogging();

        // Services for testing
        serviceCollection.AddScoped<IOptions<ApiOptions>>(_ =>
            Options.Create(ApiOptions.Create(Mt4Host, Mt4Port)));
        serviceCollection.AddTransient<ITenantService, TenantService>();
        serviceCollection.AddTransient<ITradingApiService, TradingApiService>();
        serviceCollection.AddTransient<IAccountingService, AccountingService>();
        serviceCollection.AddTransient<ISendMailService, SendMailService>();
        serviceCollection.AddScoped<IHangfireWrapper, EmptyHangfireWrapper>();
        serviceCollection.AddTransient<TradingService>();
        serviceCollection.AddTransient<ApplicationService>();
        serviceCollection.AddTransient<IRebateJob, RebateJob>();
        serviceCollection.AddTransient<IApplicationTokenService, ApplicationTokenService>();
        serviceCollection.AddTransient<ConfigurationService>();
        serviceCollection.AddSingleton<IMyCache, MyCache>();

        // S3
        serviceCollection.AddOptions<AwsS3Options>().Bind(configuration.GetSection("S3"));
        serviceCollection.AddTransient<IStorageService, AwsStorageService>();
        // Reports
        serviceCollection.AddTransient<ReportService>();

        AppServiceProvider = serviceCollection.BuildServiceProvider();
        Faker = new Faker();

        CentralDbContext = AppServiceProvider.GetRequiredService<CentralDbContext>();
        var tenant = CentralDbContext.Tenants.Include(x => x.Domains)
            .OrderBy(x => x.Id).First();
        TenancyResolver = AppServiceProvider.GetRequiredService<TenancyResolver>();
        TenancyResolver.SetTenant(tenant);
        UserManager = AppServiceProvider.GetRequiredService<UserManager<User>>();
        TokenService = AppServiceProvider.GetRequiredService<IApplicationTokenService>();
        AuthDbContext = AppServiceProvider.GetRequiredService<AuthDbContext>();
        TenantDbContext = AppServiceProvider.GetRequiredService<TenantDbContext>();
        ApplicationService = AppServiceProvider.GetRequiredService<ApplicationService>();
        TradingService = AppServiceProvider.GetRequiredService<TradingService>();
        AccountingService = AppServiceProvider.GetRequiredService<IAccountingService>();
        ConfigurationService = AppServiceProvider.GetRequiredService<ConfigurationService>();
    }

    public void Migrate()
    {
        var tenants = CentralDbContext.Tenants
            .Include(x => x.Domains)
            .OrderByDescending(x => x.Id);
        foreach (var t in tenants)
        {
            var scope = AppServiceProvider.CreateScope();
            var s = scope.ServiceProvider.GetRequiredService<TenancyResolver>();
            s.SetTenant(t);
            var exists = CentralDbContext.IsDatabaseExists(t.DatabaseName);
            if (exists) continue;

            var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            tenantCtx.Database.Migrate();
            var authCtx = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            authCtx.Database.Migrate();
            var task = Task.Run(async () =>
            {
                await tenantCtx.SeedTenantDatabase();
                await authCtx.SeedRoles();
                await authCtx.SeedDefaultUsers();
            });
            task.Wait();
        }
    }

    protected async Task<Party> GetClient()
    {
        var party = await TenantDbContext.Parties
            .Where(x => x.PartyRoles.Any(p => p.RoleId == (short)UserRoleTypes.Client))
            .OrderByDescending(x => x.Id)
            .FirstAsync();
        return party;
    }

    protected async Task<Party> GetManager()
    {
        var party = await TenantDbContext.Parties
            .Where(x => x.PartyRoles.Any(p => p.RoleId == (short)UserRoleTypes.TenantAdmin))
            .OrderByDescending(x => x.Id)
            .FirstAsync();
        return party;
    }

    /// <summary>
    /// Create a new tenant database context in memory
    /// </summary>
    /// <returns></returns>
    public static TenantDbContext CreateTenantDbContext()
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase("portal_tenant_" + Guid.NewGuid().ToString()[..6])
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        return new TenantDbContext(options.Options);
    }

    /// <summary>
    /// Create a auth database context in memory
    /// </summary>
    /// <returns></returns>
    public static AuthDbContext CreateAuthDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase("portal_auth_" + Guid.NewGuid().ToString()[..6])
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        return new AuthDbContext(options.Options);
    }

    public static DefaultHttpContext FakeHttpContext(long partyId, string requestHost = "demo.localhost")
    {
        var host = new HostString(requestHost);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(UserClaimTypes.PartyId, partyId.ToString())
        }));
        return new DefaultHttpContext
        {
            Request =
            {
                Host = host
            },
            User = user
        };
    }
}

public static class StartupExtensions
{
    public static async Task<Party> FakeParty(this TenantDbContext tenantDbContext, UserRoleTypes role)
    {
        var party = Party.Create("Fake Party " + Guid.NewGuid().ToString()[..5]);
        await tenantDbContext.Parties.AddAsync(party);
        await tenantDbContext.SaveChangesAsync();

        await tenantDbContext.PartyRoles.AddAsync(new PartyRole { PartyId = party.Id, RoleId = (int)role });
        await tenantDbContext.SaveChangesAsync();
        return party;
    }

    public static async Task<Party> FakePartyForClient(this TenantDbContext tenantDbContext)
    {
        var partyRole = Party.CreateClient("Fake Party " + Guid.NewGuid().ToString()[..5]);
        await tenantDbContext.PartyRoles.AddAsync(partyRole);
        await tenantDbContext.SaveChangesAsync();
        return partyRole.Party;
    }
}