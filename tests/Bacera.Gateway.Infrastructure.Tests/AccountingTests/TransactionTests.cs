// using Bogus;
// using Microsoft.EntityFrameworkCore;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class TransactionTests
// {
//     private readonly Party _sender;
//     private readonly Party _receiver;
//     private readonly Party _operator;
//     private readonly TenantDbContext _ctx;
//     private readonly IAccountingService _svc;
//     private const long Amount = 1999L;
//
//     public TransactionTests()
//     {
//         _ctx = Startup.CreateTenantDbContext();
//         var authCtx = Startup.CreateAuthDbContext();
//         _svc = new AccountingService(_ctx, authCtx);
//         _ctx.SeedStateMachineAsync().Wait();
//         _sender = _ctx.FakePartyForClient().Result;
//         _receiver = _ctx.FakePartyForClient().Result;
//         _operator = _ctx.FakeParty(UserRoleTypes.TenantAdmin).Result;
//     }
//
//     [Fact]
//     public async Task FullProcessTest()
//     {
//         var id = await TransactionCreateTest();
//         await TransactionCancelTest(id, _operator.Id);
//
//         id = await TransactionCreateTest();
//         await TransactionRejectTest(id);
//
//         id = await TransactionCreateTest();
//         await TransactionApproveTest(id);
//         await TransactionCompleteTest(id);
//     }
//
//     private async Task<long> TransactionCreateTest()
//     {
//         var userWallet = await _svc.WalletGetOrCreateAsync(_sender.Id, CurrencyTypes.USD);
//         userWallet.ShouldNotBeNull();
//         var account = Account.Build(_receiver.Id, AccountRoleTypes.Client);
//         await _ctx.Accounts.AddAsync(account);
//         await _ctx.SaveChangesAsync();
//
//         var tradeAccount = TradeAccount.Build(account.Id);
//         await _ctx.TradeAccounts.AddAsync(tradeAccount);
//         await _ctx.SaveChangesAsync();
//
//         var model = await _svc.TransactionCreateAsync(
//             _sender.Id, TransactionAccountTypes.Wallet, userWallet.Id,
//             _receiver.Id, TransactionAccountTypes.Account, account.Id,
//             LedgerSideTypes.Credit, (FundTypes)account.FundType, Amount, CurrencyTypes.USD, _sender.Id);
//
//         //_user.PartyId, receiver.PartyId, _user.PartyId, LedgerSideTypes.Credit, 199);
//         model.Id.ShouldBeGreaterThan(0);
//         model.IdNavigation.Id.ShouldBeGreaterThan(0);
//
//         var matter = await _ctx.Matters
//             .Where(x => x.Id == model.Id)
//             .FirstOrDefaultAsync();
//         matter.ShouldNotBeNull();
//
//         await ActivityShouldBeExists(matter.Id);
//         return model.Id;
//     }
//
//     public async Task<long> TransactionCancelTest(long transactionId, long partyId)
//     {
//         var result = await _svc.TransactionTryCancelAsync(transactionId, partyId);
//         result.ShouldBeTrue();
//         var item = await _svc.TransactionGetAsync(transactionId);
//         item.IdNavigation.StateId.ShouldBe((int)StateTypes.TransferCanceled);
//         return transactionId;
//     }
//
//     public async Task<long> TransactionRejectTest(long transactionId)
//     {
//         var item = await _svc.TransactionGetAsync(transactionId);
//         var result = await _svc.TransactionRejectByTenantAsync(item, _operator.Id);
//         result.ShouldBeTrue();
//
//         item = await _svc.TransactionGetAsync(transactionId);
//         item.IdNavigation.StateId.ShouldBe((int)StateTypes.TransferRejected);
//         return transactionId;
//     }
//
//     public async Task<long> TransactionApproveTest(long transactionId)
//     {
//         var item = await _svc.TransactionGetAsync(transactionId);
//         var result = await _svc.TransactionApproveByTenantAsync(item, _operator.Id);
//         result.ShouldBeTrue();
//
//         item = await _svc.TransactionGetAsync(transactionId);
//         item.IdNavigation.StateId.ShouldBe((int)StateTypes.TransferApproved);
//         return transactionId;
//     }
//
//     public async Task<long> TransactionCompleteTest(long transactionId)
//     {
//         var item = await _svc.TransactionGetAsync(transactionId);
//         item.IdNavigation.StateId.ShouldBe((int)StateTypes.TransferApproved);
//         var result = await _svc.TransactionCompleteAsync(item, _operator.Id);
//         result.ShouldBeTrue();
//         item = await _svc.TransactionGetAsync(transactionId);
//         item.IdNavigation.StateId.ShouldBe((int)StateTypes.TransferCompleted);
//         return transactionId;
//     }
//
//     //[Fact]
//     //public async Task FakeTransactionsTest()
//     //{
//     //    var targetUser = await userManager.Users
//     //            .OrderByDescending(x => x.Id)
//     //            .Where(x => x.PartyId == 100)
//     //            .FirstOrDefaultAsync() ?? new User();
//
//     //    var partyRole = Party.CreateClient(Faker.Internet.UserName());
//     //    await _ctx.PartyRoles.AddAsync(partyRole);
//     //    await _ctx.SaveChangesAsync();
//     //    partyRole.Party.Id.ShouldBeGreaterThan(0);
//
//     //    var receiver = FakeUser(partyRole.Party.Id);
//     //    var result = await userManager.CreateAsync(receiver);
//     //    result.Succeeded.ShouldBeTrue();
//
//     //    var ids = new List<long>();
//
//     //    for (var i = 0; i < 20; i++)
//     //    {
//     //        var model = await _svc.TransactionCreateAsync(
//     //            targetUser.PartyId, receiver.PartyId, targetUser.PartyId, LedgerSideTypes.Credit, 199);
//     //        model.Id.ShouldBeGreaterThan(0);
//     //        ids.Add(model.Id);
//     //    }
//
//     //    var transactions = await _ctx.Transactions
//     //        .Where(x => ids.Contains(x.Id))
//     //        .Include(x => x.Matter)
//     //        .ToListAsync();
//     //    foreach (var transaction in transactions)
//     //    {
//     //        transaction.Matter.StatedOn = Faker.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now);
//     //        transaction.Matter.StateId = (int)StateTypes.TransferCompleted;
//
//     //    }
//     //    _ctx.Transactions.UpdateRange(transactions);
//     //    await _ctx.SaveChangesAsync();
//     //}
//
//
//     //[Fact]
//     //public async Task FakeTransactionsWithClientsTest()
//     //{
//     //    var targetUser = await userManager.Users
//     //        .OrderByDescending(x => x.Id)
//     //        .FirstOrDefaultAsync() ?? new User();
//
//     //    var clientPartyIds = await _ctx.Parties
//     //        .Where(x => x.PartyRoles.Any(y => y.RoleId == (int)UserRoleTypes.Client))
//     //        .Select(x => x.Id)
//     //        .Take(200)
//     //        .ToListAsync();
//
//
//     //    var ids = new List<long>();
//
//     //    for (var i = 0; i < 100; i++)
//     //    {
//     //        var model = await _svc.TransactionCreateAsync(
//     //            targetUser.PartyId,
//     //            Faker.PickRandom(clientPartyIds),
//     //            targetUser.PartyId,
//     //            LedgerSideTypes.Credit,
//     //            Faker.Random.Long(199, 28190));
//     //        model.Id.ShouldBeGreaterThan(0);
//     //        ids.Add(model.Id);
//     //    }
//
//     //    var transactions = await _ctx.Transactions
//     //        .Where(x => ids.Contains(x.Id))
//     //        .Include(x => x.Matter)
//     //        .ToListAsync();
//     //    foreach (var transaction in transactions)
//     //    {
//     //        transaction.Matter.StatedOn = Faker.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now);
//     //        transaction.Matter.StateId = (int)StateTypes.TransferCompleted;
//
//     //        _ctx.Matters.UpdateRange(transaction.Matter);
//     //        await _ctx.SaveChangesAsync();
//
//     //    }
//     //}
//
//
//     private async Task ActivityShouldBeExists(long matterId)
//     {
//         var hasActivity = await _ctx.Activities
//             .Where(x => x.MatterId == matterId)
//             .AnyAsync();
//         hasActivity.ShouldBeTrue();
//     }
// }

