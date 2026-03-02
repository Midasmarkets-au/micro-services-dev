using System.Net;
using System.Security.Authentication;
using Bacera.Gateway.Vendor.MetaTrader;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class CopyTradeTests : Startup
{
    private const string Login = "1090";
    private const string Pass = "abc123";
    private const string Url = "https://202.66.136.54:8850";

    private readonly TenantDbContext _ctx;
    private readonly CopyTradeService _svc;
    private const int TestTimeout = 5000;

    public CopyTradeTests()
    {
        var option = Options.Create(new CopyTradeOptions
        {
            Endpoint = Url,
            Login = Login,
            Password = Pass,
        });
        var ctx = ServiceProvider.GetRequiredService<TenantDbContext>();
        _svc = new CopyTradeService(option, ctx);
        _ctx = ctx;
    }

    [Fact(Timeout = TestTimeout)]
    public async Task ListTest()
    {
        var result = await _svc.ListRulesByApiAsync();
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        var first = result.First();
        first.Id.ShouldBeGreaterThan(0);
        first.TargetList.Count.ShouldBeGreaterThan(0);
        first.TargetList.First().ShouldNotBeNull();
        first.TargetList.First().TargetAccounts.Count.ShouldBeGreaterThan(0);
    }

    [Fact(Timeout = TestTimeout)]
    public async Task CreateTest()
    {
        var result = await _svc.CreateAsync(1362479002, 1362479004, ModeTypes.ByEquity);
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBeGreaterThan(0);
    }

    [Fact(Timeout = TestTimeout)]
    public async Task CreateRuleByApiTest()
    {
        var result = await _svc.CreateRuleByApiAsync(1362479002, 1362479004, ModeTypes.ByEquity);
        result.ShouldBeGreaterThan(0);
    }

    [Fact(Timeout = TestTimeout)]
    public async Task DeleteTest()
    {
        var result = await _svc.DeleteRuleByApiAsync(6);
        result.ShouldBeTrue();
    }

    [Fact(Timeout = TestTimeout)]
    public async Task SslConnectTest()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback += (_, _, _, _) => true;
        handler.SslProtocols = SslProtocols.None; // for match API Server TLS version 1.0
        var client = new HttpClient(handler);
        var result = await client.GetAsync(Url + "/rules/");
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        result.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact(Timeout = TestTimeout)]
    public async Task QueryTest()
    {
        var criteria = new CopyTrade.Criteria();
        var result = await _svc.QueryAsync(criteria);
        result.ShouldNotBeNull();
    }
}