using Bacera.Gateway.Auth;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

partial class AuthDbContext
{
    public async Task<int> SaveChangesWithAuditAsync(long partyId)
    {
        return await BaseDbContext.SaveChangesWithAuditAsync<UserAudit>(this, partyId,
            async () => await base.SaveChangesAsync());
    }
}

public static class AuthDbContextExtensions
{
    public static async Task AppendUserInfo<T>(this AuthDbContext ctx, List<T> items)
        where T : class, IUserInfoAppendable
    {
        var users = await ctx.Users
            .Where(x => items.Select(y => y.PartyId).Contains(x.PartyId))
            .ToUserInfo()
            .ToListAsync();

        foreach (var item in items)
        {
            var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
            if (user != null)
                item.User = user;
        }
    }
}