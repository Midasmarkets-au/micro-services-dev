using Bacera.Gateway.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class MT5DatabaseContextTests : Startup
{
    private readonly MetaTrade5DbContext _ctx;

    public MT5DatabaseContextTests()
    {
        _ctx = ServiceProvider.GetRequiredService<MetaTrade5DbContext>();
    }

    [Fact]
    public async Task FilterByCriteria_ShouldBeNotNull()
    {
        // Act
        var criteria = new TradeViewModel.Criteria { AccountNumber = 15501, Size = 3 };

        var query = _ctx.Mt5DealsViews.AsQueryable().ToTradeViewModel();
        var result = await query.PagedFilterBy(criteria).ToListAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }
}