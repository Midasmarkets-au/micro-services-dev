using Bacera.Gateway;
using Bacera.Gateway.Integration;
using Microsoft.EntityFrameworkCore;

namespace Cleaner;

public class TicketRegenerate
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly MetaTrade5DbContext _mt5DbContext;

    public TicketRegenerate(TenantDbContext tenantDbContext, MetaTrade5DbContext mt5DbContext)
    {
        _tenantDbContext = tenantDbContext;
        _mt5DbContext = mt5DbContext;
    }

    public async Task<(bool, string)> Run()
    {
        var mt5TradeRebate = await _tenantDbContext.TradeRebates
            .Where(x => x.TradeServiceId == 30)
            .ToListAsync();

        var dealIds = mt5TradeRebate.Select(x => (ulong)x.DealId).ToList();

        var closedDeals = await _mt5DbContext.Mt5Deals2025s
            .Where(x => dealIds.Contains(x.Deal))
            .Select(x => new { x.Deal, x.PositionId, x.Order })
            .ToListAsync();

        var openPositions = await _mt5DbContext.Mt5Deals2025s
            .Where(x => closedDeals.Select(d => d.PositionId).Contains(x.PositionId))
            .Where(x => x.VolumeClosed == 0)
            .Select(x => new { x.PositionId, x.TimeMsc })
            .ToListAsync();

        var count = 0;
        foreach (var item in mt5TradeRebate)
        {
            var deal = closedDeals.FirstOrDefault(x => (long)x.Deal == item.DealId);
            if (deal == null) continue;

            var pos = openPositions.FirstOrDefault(x => x.PositionId == deal.PositionId);
            if (pos == null) continue;

            Console.WriteLine($"Update {item.Id} from legacy ticket {item.Ticket} to {deal.Order}");
            item.OpenedOn = DateTime.SpecifyKind(pos.TimeMsc, DateTimeKind.Utc);

            item.Ticket = (long)deal.Order;
            count++;
            _tenantDbContext.TradeRebates.Update(item);
            await _tenantDbContext.SaveChangesAsync();
            // break;
        }

        return (true, $"Updated {count} records.\n");
    }
}