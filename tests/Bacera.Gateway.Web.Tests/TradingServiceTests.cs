using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Web.Tests;

public class TradingServiceTests : Startup
{
    public TradingServiceTests()
    {
    }

    // [Fact]
    // public async Task ListTenant()
    // {
    //     var tenants = await centralDbContext.Tenants.ToListAsync();
    //     foreach (var tenant in tenants)
    //     {
    //         tenancyResolver.SetTenant(tenant);
    //         var count = await tenantDbContext.Topics.CountAsync();
    //         count.ShouldBeGreaterThan(0);
    //     }
    // }


    [Fact]
    public async Task TradeQuery_ShouldNotBeNull()
    {
    }
}