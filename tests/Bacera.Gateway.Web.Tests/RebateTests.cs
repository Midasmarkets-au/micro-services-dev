using Bacera.Gateway.Web.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Web.Tests;

public class RebateTests : Startup
{
    private readonly IRebateJob _job;
    private readonly TradingService _svc;

    public RebateTests()
    {
        _job = AppServiceProvider.GetRequiredService<IRebateJob>();
        _svc = AppServiceProvider.GetRequiredService<TradingService>();
    }

    [Fact]
    public async Task CalculateRebate_ForAllTenant_ShouldNotBeNull()
    {
        var result = await _job.CalculateRebate();
        result.ShouldNotBeNull();
    }


    //
    // [Fact]
    // public async Task GetAgentRebateRuleTree_ForAllTenant_ShouldNotBeNull()
    // {
    //     var items = await _svc.GetAgentRebateRuleTree(12987);
    //     items.ShouldNotBeNull();
    //     items.Count.ShouldBeGreaterThan(1);
    // }
}