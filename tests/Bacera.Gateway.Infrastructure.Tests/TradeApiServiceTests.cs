using Bacera.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class TradeApiServiceTests : Startup
{
    private readonly TradingApiService _svc;
    private const string Group = "demoUSD_STN";
    private const string Password = "Pass!1234";

    public TradeApiServiceTests()
    {
        TenantDbContext.SeedTradeServiceAsync().Wait();
        _svc = ServiceProvider.GetRequiredService<TradingApiService>();
    }

    [Fact]
    public async Task CreateAccountTest()
    {
        var guid = Guid.NewGuid().ToString("N");
        var account =
            await _svc.CreateAccountAsync(PlatformTypes.MetaTrader4Demo, "test-demo-" + guid, Password, 200, Group, null);
        account.AccountNumber.ShouldBeGreaterThan(0);

        var tradeSever = await TenantDbContext.TradeServices.FindAsync(account.ServiceId);
        tradeSever.ShouldNotBeNull();
        tradeSever.Platform.ShouldBe((short)PlatformTypes.MetaTrader4Demo);


        guid = Guid.NewGuid().ToString("N");
        account = await _svc.CreateAccountAsync(PlatformTypes.MetaTrader4, "test-" + guid, Password, 200, Group, null);
        account.AccountNumber.ShouldBeGreaterThan(0);

        tradeSever = await TenantDbContext.TradeServices.FindAsync(account.ServiceId);
        tradeSever.ShouldNotBeNull();
        tradeSever.Platform.ShouldBe((short)PlatformTypes.MetaTrader4);
    }
}