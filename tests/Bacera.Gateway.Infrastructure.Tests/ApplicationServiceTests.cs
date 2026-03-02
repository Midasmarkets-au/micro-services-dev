using Bacera.Gateway.Auth;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class ApplicationServiceTests : Startup
{
    private readonly ApplicationService _svc;

    public ApplicationServiceTests()
    {
        _svc = ServiceProvider.GetRequiredService<ApplicationService>();
        _svc.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateApplicationTest()
    {
        var party = await FakePartyForClient();
        var supplement = ApplicationSupplement.Build(AccountRoleTypes.Client, FundTypes.Wire);
        var item = await _svc.CreateApplication(party.Id, ApplicationTypes.TradeDemoAccount, supplement);
        item.ShouldNotBeNull();
        item.Type.ShouldBe((short)ApplicationTypes.TradeDemoAccount);
    }
}