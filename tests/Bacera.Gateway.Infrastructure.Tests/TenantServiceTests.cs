using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

using M = Tenant;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class TenantServiceTests : Startup
{
    private readonly ITenantService _svc;

    public TenantServiceTests()
    {
        _svc = ServiceProvider.GetRequiredService<ITenantService>();
    }

    // [Fact]
    // public async Task CreateTenantTest()
    // {
    //     var tenant = await _svc.CreateAsync(
    //         Faker.Internet.UserName(),
    //         "portal_tenant_testing_" + Faker.Internet.UserName().ToLower(),
    //         Faker.Internet.DomainWord() + ".localhost");
    //     tenant.Id.ShouldBeGreaterThan(0);
    //     tenant.Domains.ShouldNotBeNull();
    //
    //     var exists = centralDbContext.IsDatabaseExists(tenant.DatabaseName);
    //     exists.ShouldBeTrue();
    // }

    [Fact]
    public async Task PaginationTest()
    {
    }
}