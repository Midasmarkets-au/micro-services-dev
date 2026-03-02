using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Infrastructure)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class DatabaseMigrateTests
{
    private const string DatabaseName = "portal_tenant_development";
    private readonly ServiceProvider _serviceProvider;

    public DatabaseMigrateTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContext<CentralDbContext>(options
            => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        serviceCollection.AddDbContext<AuthDbContext>((services, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("TenantConnectionTemplate")!
                .Replace("{{DATABASE}}", DatabaseName));
        });

        serviceCollection.AddDbContext<TenantDbContext>((services, options) =>
        {
            var connectionString = configuration.GetConnectionString("TenantConnectionTemplate")!
                .Replace("{{DATABASE}}", DatabaseName);
            options.UseNpgsql(connectionString);
        });
        serviceCollection.AddLogging();
        _serviceProvider = serviceCollection.BuildServiceProvider();

        var authCtx = _serviceProvider.GetRequiredService<AuthDbContext>();
        var tenantCtx = _serviceProvider.GetRequiredService<TenantDbContext>();
    }

    [Fact]
    public async Task CentralDatabaseMigrate()
    {
        var centralDbContext = _serviceProvider.GetRequiredService<CentralDbContext>();
        var dbExists = await centralDbContext.Database.EnsureCreatedAsync();
        dbExists.ShouldBeFalse();
    }

    [Fact]
    public async Task TenantDatabaseMigrate()
    {
        var authCtx = _serviceProvider.GetRequiredService<AuthDbContext>();
        await authCtx.Database.EnsureDeletedAsync();

        await authCtx.Database.EnsureCreatedAsync();
        if (!authCtx.Roles.Any())
        {
            await authCtx.SeedRoles();
            await authCtx.SeedDefaultUsers();
        }

        authCtx.Roles.Any().ShouldBeTrue();

        var tenantCtx = _serviceProvider.GetRequiredService<TenantDbContext>();
        await tenantCtx.Database.MigrateAsync();
        if (!tenantCtx.Parties.Any())
        {
            await tenantCtx.SeedTenantDatabase();
        }

        tenantCtx.Parties.Any().ShouldBeTrue();
    }
}