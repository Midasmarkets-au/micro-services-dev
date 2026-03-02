using Bacera.Gateway.Auth;
using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class UserManagerTests : Startup
{
    // private readonly AuthDbContext _ctx;
    private const string Password = "Pass!1234";

    public UserManagerTests()
    {
    }

    [Fact]
    public async Task AddUserTest()
    {
        TenancyResolver.Tenant.ShouldNotBeNull();
        TenancyResolver.Tenant.Id.ShouldBeGreaterThan(0);

        var username = Faker.Internet.UserName();
        var party = Party.Create(username);
        await TenantDbContext.Parties.AddAsync(party);
        await TenantDbContext.SaveChangesAsync();
        party.Id.ShouldBeGreaterThan(0);

        var user = new User
        {
            TenantId = TenancyResolver.Tenant.Id,
            PartyId = party.Id,
            UserName = username,
            Email = Faker.Internet.Email(),
            EmailConfirmed = true,
        };
        var result = await UserManager.CreateAsync(user);
        result.Succeeded.ShouldBeTrue();
        user.ShouldNotBeNull();
        user.Id.ShouldBeGreaterThan(0);

        await UserManager.RemovePasswordAsync(user);
        await UserManager.AddPasswordAsync(user, Password);
    }

    [Fact]
    public async Task UpdateUserTest()
    {
        var userIds = new long[] { 1, 2, 3 };
        var users = await UserManager.Users
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            await UserManager.RemovePasswordAsync(user);
            var hasPassword = await UserManager.HasPasswordAsync(user);
            hasPassword.ShouldBeFalse();
            await UserManager.AddPasswordAsync(user, Password);
            hasPassword = await UserManager.HasPasswordAsync(user);
            hasPassword.ShouldBeTrue();
            user.EmailConfirmed = true;
            await UserManager.UpdateAsync(user);
        }
    }

    //[Fact]
    [Fact(Skip = "Fix users data only")]
    public async Task UpdateUidTest()
    {
        var users = await AuthDbContext.Users
            .Where(x => x.Uid == 0)
            .ToListAsync();

        foreach (var user in users)
        {
            user.Uid = await Utils.GenerateUniqueIdAsync(IsUidExits);
            AuthDbContext.Users.Update(user);
            //await authDbContext.SaveChangesWithAuditAsync(100);
            await AuthDbContext.SaveChangesAsync();
        }
    }

    private async Task<bool> IsUidExits(long uid) => await AuthDbContext.Users.AnyAsync(x => x.Uid == uid);
}