using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

partial class TenantDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=portal_tenant_development;Username=postgres;Password=dev;");
        }

        optionsBuilder
            .EnableSensitiveDataLogging(); // 可选，允许记录敏感数据
    }

    public async Task<int> SaveChangesWithAuditAsync(long partyId)
    {
        return await BaseDbContext.SaveChangesWithAuditAsync<Audit>(this, partyId,
            async () => await base.SaveChangesAsync());
    }
}

//public static class TenantDbContextExtension
//{
//    public static async Task FulfillTags(this TenantDbContext context, ICollection<IHasPartyTags> items)
//    {
//        var partyIds = items.Select(x => x.PartyId).Distinct().ToList();
//        var parties = await context.Parties
//            .Where(x => partyIds.Contains(x.Id))
//            .Select(x => new Party { Id = x.Id, Tags = x.Tags })
//            .ToListAsync();

//        foreach (var item in items)
//        {
//            item.Tags = parties.Single(x => x.Id == item.PartyId).Tags.ToList();
//        }
//    }
//}