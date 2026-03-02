using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Bacera.Gateway.Console.Migrate.Tests;


[Trait(TraitTypes.Types, TraitTypes.Value.Infrastructure)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class ParseUserTest : Initial
{
    [Fact]
    public async Task HaveUsersTest()
    {
        var hasUser = await LegacyCtx.Users.AnyAsync();
        hasUser.ShouldBeTrue();
    }

    [Fact]
    public async Task GetSingleUsersTest()
    {
        var user = await LegacyCtx.Users
            .Where(x => !string.IsNullOrEmpty(x.Email))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync( x=>x.Id == 1);
        user.ShouldNotBeNull();

        var personInfos = await LegacyCtx.PersonalInfos
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        var demoAccounts = await LegacyCtx.DemoAccounts
            .Where(x => x.UserId == user.Id)
            .Where(x => x.ExpiredAt > DateTime.UtcNow)
            .ToListAsync();

        var financialInfos = await LegacyCtx.FinancialInfos
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        var documents = await LegacyCtx.Documents
            .Where(x => x.SubjectType == "App\\User")
            .Where(x => x.SubjectId == user.Id)
            .ToListAsync();

        var clientComments = await LegacyCtx.ClientComments
            .Where(x => x.UserId == user.Id)
            .ToListAsync();
    }
}