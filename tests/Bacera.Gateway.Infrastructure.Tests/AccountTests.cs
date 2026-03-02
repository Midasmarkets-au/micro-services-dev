// using System.Diagnostics;
// using Microsoft.EntityFrameworkCore;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Infrastructure)]
// public class AccountTests : Startup
// {
//     [Fact]
//     public async Task GetAgentListTest()
//     {
//         if (tenantDbContext.Database.IsInMemory())
//         {
//             true.ShouldBeTrue("Skipped when using In Memory Database");
//             return;
//         }
//
//         var accountId = 10114L;
//         var sw = new Stopwatch();
//         sw.Start();
//         var parentAccounts = await tenantDbContext.Accounts.GetAllParentAccountsAsync(accountId);
//         sw.Stop();
//         var elapsed = sw.ElapsedMilliseconds;
//         Assert.NotEmpty(parentAccounts);
//         Debug.WriteLine(elapsed);
//     }
//
//     //[Fact]
//     //public async Task Assign100AgentTest()
//     //{
//     //    while (tenantDbContext.Accounts.Count(x => x.Aid == null) > 10)
//     //    {
//     //        var accounts = await tenantDbContext.Accounts
//     //            .Where(x => x.Aid == null)
//     //            .OrderBy(x => x.Id)
//     //            .Take(Faker.Random.Number(3, 12)).ToListAsync();
//     //        var aid = accounts.First().Id;
//     //        foreach (var account in accounts.Skip(1))
//     //        {
//     //            account.Aid = aid;
//     //            tenantDbContext.Accounts.Update(account);
//
//     //            var distributionRule = new RebateDistributionRule
//     //            {
//     //                SourceAccountId = account.Id,
//     //                TargetAccountId = aid,
//     //                Percentage = (decimal)Faker.Random.Number(3, 30) / 100,
//     //            };
//
//     //            tenantDbContext.RebateDistributionRules.Add(distributionRule);
//     //            await tenantDbContext.SaveChangesAsync();
//     //            aid = account.Id;
//     //        }
//     //    }
//     //}
// }
//
// public static class AccountTestsExtensions
// {
//     public static async Task<List<Account>> GetAllParentAccountsAsync(this IQueryable<Account> query, long id)
//     {
//         var parentAccounts = new List<Account>();
//         var currentAccount = await query.SingleOrDefaultAsync(a => a.Id == id);
//
//         while (currentAccount is { AgentAccountId: > 0 })
//         {
//             currentAccount = await query.SingleOrDefaultAsync(a => a.Id == currentAccount.AgentAccountId);
//             if (currentAccount != null)
//             {
//                 parentAccounts.Add(currentAccount);
//             }
//         }
//
//         return parentAccounts;
//     }
// }