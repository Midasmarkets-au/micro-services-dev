// using Bacera.Gateway.Auth;
//
// using Microsoft.EntityFrameworkCore;
//
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class AccountingRebateTests : Startup
// {
//     private readonly Party _party;
//     private readonly Party _operator;
//     private readonly IAccountingService _svc;
//
//     public AccountingRebateTests()
//     {
//         _party = FakePartyForClient("Rebate Test Client").Result;
//         _operator = FakeParty(UserRoleTypes.System, "Rebate Test admin").Result;
//         TenantDbContext.SeedStateMachineAsync().Wait();
//         AuthDbContext.SeedRoles().Wait();
//         AuthDbContext.SeedDefaultUsers().Wait();
//         _svc = new AccountingService(TenantDbContext, AuthDbContext);
//     }
//
//     [Fact]
//     public async Task RebateCreateTest()
//     {
//         var account = await FakeClientAccount();
//         var rebate =
//             await _svc.RebateCreateAsync(account.PartyId, account.Id, 0, 199, CurrencyTypes.USD, _operator.Id);
//         rebate.Id.ShouldBeGreaterThan(0);
//         rebate.IdNavigation.StateId.ShouldBe((int)StateTypes.RebateCreated);
//     }
//
//     [Fact]
//     public async Task RebateCompleteTest()
//     {
//         var account = await FakeClientAccount();
//         var rebate = await _svc.RebateCreateAsync(account.PartyId, account.Id, 0, 199, CurrencyTypes.USD, _operator.Id);
//         rebate.Id.ShouldBeGreaterThan(0);
//         rebate.IdNavigation.StateId.ShouldBe((int)StateTypes.RebateCreated);
//
//         var result = await _svc.RebateHoldAsync(rebate, _operator.Id);
//         result.ShouldBeTrue();
//         result = await _svc.RebateReleaseAsync(rebate, _operator.Id);
//         result.ShouldBeTrue();
//         result = await _svc.RebateCompleteAsync(rebate, _operator.Id);
//         result.ShouldBeTrue();
//         var matter = await TenantDbContext.Matters
//             .Where(x => x.Id == rebate.Id)
//             .FirstOrDefaultAsync();
//         matter.ShouldNotBeNull();
//         matter.StateId.ShouldBe((int)StateTypes.RebateCompleted);
//     }
//
//     //[Fact]
//     //public async Task AddRebatesTest()
//     //{
//     //    var ibAccountId = 11090;
//     //    var account = tenantDbContext.Accounts
//     //        .Where(x => x.PartyId > 0 && x.Role == (int)AccountRoleTypes.Ib)
//     //        .FirstOrDefault(x => x.Id == ibAccountId);
//     //    account.ShouldNotBeNull();
//     //    for (var i = 0; i < 20; i++)
//     //    {
//     //        var rebate = await _svc.RebateCreateAsync(account.PartyId, account.Id, 0, 199, CurrencyTypes.USD, 0);
//     //        rebate.Id.ShouldBeGreaterThan(0);
//     //        var rebateComplete = await _svc.RebateCompleteAsync(rebate, _user.PartyId);
//     //        rebateComplete.ShouldBeTrue();
//     //    }
//     //}
//
//     //[Fact]
//     //public async Task ArrangeRebatesDateTest()
//     //{
//     //    var ibAccountId = 11090;
//     //    var rebates = await tenantDbContext.Rebates
//     //        .Where(x => x.Matter.StateId == (int)StateTypes.RebateCompleted)
//     //        .Where(x => x.AccountId == ibAccountId)
//     //        .Include(x => x.Matter)
//     //        .ToListAsync();
//     //    foreach (var rebate in rebates)
//     //    {
//     //        rebate.Amount = Faker.Random.Long(199, 90000);
//     //        rebate.Matter.StatedOn = Faker.Date.Between(DateTime.Now.AddDays(-45), DateTime.Now);
//     //        tenantDbContext.Rebates.Update(rebate);
//     //        await tenantDbContext.SaveChangesAsync();
//     //    }
//     //}
//
//
//     private async Task<Account> FakeClientAccount()
//     {
//         var party = await FakePartyForClient("Fake Client");
//         var account = Account.Build(party.Id, AccountRoleTypes.Client);
//         account.Uid = (new Random()).Next(230100000, 230999999);
//         await TenantDbContext.Accounts.AddAsync(account);
//         await TenantDbContext.SaveChangesAsync();
//         return account;
//     }
// }

