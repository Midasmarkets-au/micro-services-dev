using Amazon;
using Bacera.Gateway.Services;
using Bacera.Gateway.Vendor.Amazon;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Bacera.Gateway.Vendor.Amazon.Options;
using Bacera.Gateway;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Vendor.MetaTrader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using StackExchange.Redis;

namespace Bacera.Gateway.Infrastructure.Tests;

public class Startup
{
    protected readonly Faker Faker;
    protected readonly ServiceProvider ServiceProvider;
    protected readonly TenancyResolver TenancyResolver;
    protected readonly UserManager<User> UserManager;
    protected readonly TenantDbContext TenantDbContext;
    protected readonly AuthDbContext AuthDbContext;
    protected readonly CentralDbContext CentralDbContext;
    protected readonly IAccountingService AccountingService;

    protected const int Mt4Port = 4578;
    protected const string Mt4Host = "13.57.233.166";
    protected readonly IConfigurationRoot Configuration;

    /**
     * Migrate:
     * dotnet ef database update -c AuthDbContext --connection  "Host=localhost;Database=portal_central_testing;Username=postgres;Password=dev"
     * dotnet ef database update -c CentralDbContext --connection  "Host=localhost;Database=portal_central_testing;Username=postgres;Password=dev"
     */
    protected Startup()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<TenancyResolver>();
        serviceCollection.AddDistributedMemoryCache();
        serviceCollection.AddScoped(typeof(ITenantGetter), svc => svc.GetRequiredService<TenancyResolver>());
        serviceCollection.AddScoped(typeof(ITenantSetter), svc => svc.GetRequiredService<TenancyResolver>());

        serviceCollection.AddScoped(_ =>
            Options.Create(TenantDatabaseOptions.Create("localhost", "", "postgres", "dev")));

        serviceCollection.AddDbContext<CentralDbContext>(options
                => options.UseInMemoryDatabase("portal_central")
                    // don't raise the error warning us that the in memory db doesn't support transactions
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            // => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        serviceCollection.AddDbContext<AuthDbContext>((services, options)
            //=>
            // {
            //
            //     // var tenant = services.GetRequiredService<ITenantGetter>().Tenant;
            //     // options.UseNpgsql(configuration.GetConnectionString("TenantConnectionTemplate")!
            //     //     .Replace("{{DATABASE}}", tenant.DatabaseName));
            // }
            => options.UseInMemoryDatabase("portal_tenant")
                // don't raise the error warning us that the in memory db doesn't support transactions
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        );

        serviceCollection.AddDbContext<TenantDbContext>((services, options)
            //         =>
            // {
            //     var tenant = services.GetRequiredService<ITenantGetter>().Tenant;
            //     var connectionString = configuration.GetConnectionString("TenantConnectionTemplate")!
            //         .Replace("{{DATABASE}}", tenant.DatabaseName);
            //     options.UseNpgsql(connectionString);
            // }
            => options.UseInMemoryDatabase("portal_tenant")
                // don't raise the error warning us that the in memory db doesn't support transactions
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        );
        serviceCollection.AddDbContext<MetaTrade5DbContext>(
            options => options.UseMySql(Configuration.GetConnectionString("MT5Connection"),
                ServerVersion.Parse("5.7.38-mysql"))
        );

        serviceCollection.AddIdentity<User, ApplicationRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddUserManager<UserManager<User>>();

        serviceCollection.AddLogging();
        serviceCollection.Configure<CopyTradeOptions>(options => Configuration.GetSection("MetaTrader5").Bind(options));

        // add Redis
        var mockEnvironment = new Mock<IHostingEnvironment>();
        //...Setup the mock as needed
        mockEnvironment
            .Setup(m => m.EnvironmentName)
            .Returns("Hosting:UnitTestEnvironment");
        serviceCollection.AddSingleton(mockEnvironment.Object);

        serviceCollection.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")!));
        serviceCollection.AddSingleton<IMyCache, MyCache>();

        // Services for testing
        serviceCollection.AddScoped<IOptions<ApiOptions>>(_ =>
            Options.Create(ApiOptions.Create(Mt4Host, Mt4Port)));
        serviceCollection.AddTransient<ITenantService, TenantService>();
        serviceCollection.AddTransient<IAccountingService, AccountingService>();
        serviceCollection.AddTransient<ITradingApiService, TradingApiService>();
        serviceCollection.AddTransient<TradingService>();
        serviceCollection.AddTransient<ApplicationService>();
        serviceCollection.AddTransient<AccountingService>();
        serviceCollection.AddTransient<ConfigurationService>();
        serviceCollection.AddTransient<TradingApiService>();
        serviceCollection.Configure<AwsSesOptions>(options =>
        {
            var cfg = Configuration.GetSection("SES");
            options.AccessKey = cfg["AccessKey"] ?? throw new ArgumentNullException(nameof(options.AccessKey));
            options.SecretKey = cfg["SecretKey"] ?? throw new ArgumentNullException(nameof(options.SecretKey));
            options.Region =
                RegionEndpoint.GetBySystemName(cfg["Region"] ??
                                               throw new ArgumentNullException(nameof(options.Region)));
            options.FromAddress = cfg["FromAddress"] ?? throw new ArgumentNullException(nameof(options.FromAddress));
        });
        serviceCollection.Configure<AwsS3Options>(Configuration.GetSection("S3"));
        serviceCollection.AddTransient<IEmailSender, AwsEmailSender>();
        serviceCollection.AddTransient<ISendMailService, SendMailService>();

        ServiceProvider = serviceCollection.BuildServiceProvider();

        CentralDbContext = ServiceProvider.GetRequiredService<CentralDbContext>();

        var tenant = new Tenant { Id = 1, Name = "Test", DatabaseName = "portal_tenant" };
        TenancyResolver = ServiceProvider.GetRequiredService<TenancyResolver>();
        TenancyResolver.SetTenant(tenant);

        UserManager = ServiceProvider.GetRequiredService<UserManager<User>>();
        TenantDbContext = ServiceProvider.GetRequiredService<TenantDbContext>();
        AuthDbContext = ServiceProvider.GetRequiredService<AuthDbContext>();
        AccountingService = ServiceProvider.GetRequiredService<AccountingService>();

        Faker = new Faker();
        //Migrate();
    }

    public void Migrate()
    {
        var tenants = CentralDbContext.Tenants
            .Include(x => x.Domains)
            .OrderByDescending(x => x.Id);
        foreach (var t in tenants)
        {
            var scope = ServiceProvider.CreateScope();
            var s = scope.ServiceProvider.GetRequiredService<TenancyResolver>();
            s.SetTenant(t);

            var createContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            createContext.Database.EnsureCreated();

            var createAuthContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            createAuthContext.Database.EnsureCreated();

            var authCtx = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            try
            {
                authCtx.Database.MigrateAsync().Wait();
            }
            catch
            {
                //
            }

            try
            {
                tenantCtx.Database.MigrateAsync().Wait();
            }
            catch
            {
                //
            }

            if (authCtx.Users.Any() == false)
            {
                authCtx.SeedRoles().Wait();
                authCtx.SeedDefaultUsers().Wait();
            }

            if (tenantCtx.Parties.Any() == false)
            {
                tenantCtx.SeedTenantDatabase().Wait();
            }
        }
    }

    public User FakeUser(long partyId) =>
        new()
        {
            PartyId = partyId,
            UserName = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            CreatedOn = DateTime.UtcNow,
            LastLoginOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            EmailConfirmed = true,
            SecurityStamp = "L2C3IAVED3PCCHDZIF5K7GR25ZVJMVJT",
            Uid = new Random().Next(10000900, 400000000),
        };

    public async Task<Party> FakeParty(UserRoleTypes role, string username = "")
    {
        var party = Party.Create(string.IsNullOrEmpty(username) ? Faker.Internet.UserName() : username);
        await TenantDbContext.Parties.AddAsync(party);
        await TenantDbContext.SaveChangesAsync();

        await TenantDbContext.PartyRoles.AddAsync(new PartyRole { PartyId = party.Id, RoleId = (int)role });
        await TenantDbContext.SaveChangesAsync();
        return party;
    }

    public async Task<Party> FakePartyForClient(string username = "")
    {
        username = string.IsNullOrEmpty(username) ? Faker.Internet.UserName() : username;
        var partyRole = Party.CreateClient(username);
        await TenantDbContext.PartyRoles.AddAsync(partyRole);
        await TenantDbContext.SaveChangesAsync();
        return partyRole.Party;
    }

    public static TenantDbContext CreateTenantDbContext()
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase("portal_tenant_" + Guid.NewGuid().ToString()[..6])
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        return new TenantDbContext(options.Options);
    }

    public static AuthDbContext CreateAuthDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase("portal_tenant_" + Guid.NewGuid().ToString()[..6])
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        return new AuthDbContext(options.Options);
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