using Microsoft.EntityFrameworkCore;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Utility)]
public class AuditTests : Startup
{
    private readonly Party _party;

    public AuditTests()
    {
        _party = FakePartyForClient().Result;
    }

    [Fact]
    public async Task AccountAuditTest()
    {
        var account = Account.Build(_party.Id, AccountRoleTypes.Client);
        await TenantDbContext.Accounts.AddAsync(account);
        await TenantDbContext.SaveChangesAsync();
        account.ShouldNotBeNull();

        var groupName = Guid.NewGuid().ToString()[..10];
        account.Group = groupName;
        TenantDbContext.Accounts.Update(account);
        await TenantDbContext.SaveChangesWithAuditAsync(_party.Id);

        var audit = await TenantDbContext.Audits
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.RowId == account.Id && x.Type == (int)AuditTypes.Account);
        audit.ShouldNotBeNull();
    }

    [Fact]
    public async Task TradeAccountAuditTest()
    {
        var baseAccount = Account.Build(_party.Id, AccountRoleTypes.Client);
        await TenantDbContext.Accounts.AddAsync(baseAccount);
        await TenantDbContext.SaveChangesAsync();
        baseAccount.ShouldNotBeNull();

        var tradeAccount = TradeAccount.Build(baseAccount.Id);
        await TenantDbContext.TradeAccounts.AddAsync(tradeAccount);
        await TenantDbContext.SaveChangesAsync();
        tradeAccount.ShouldNotBeNull();
        
        TenantDbContext.TradeAccounts.Update(tradeAccount);
        await TenantDbContext.SaveChangesWithAuditAsync(_party.Id);

        var audit = await TenantDbContext.Audits
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.RowId == tradeAccount.Id && x.Type == (int)AuditTypes.TradeAccount);
        audit.ShouldNotBeNull();
    }

    [Fact]
    public async Task UserAuditTest()
    {
        var user = Auth.User.Create("emil@gmail.com");
        await AuthDbContext.Users.AddAsync(user);
        await AuthDbContext.SaveChangesAsync();
        user.Id.ShouldBeGreaterThan(0);

        user.PhoneNumber = Guid.NewGuid().ToString()[..10];
        AuthDbContext.Users.Update(user);
        await AuthDbContext.SaveChangesWithAuditAsync(_party.Id);

        var audit = await AuthDbContext.UserAudits
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.RowId == user.Id && x.Type == (int)AuditTypes.User);
        audit.ShouldNotBeNull();
    }
}