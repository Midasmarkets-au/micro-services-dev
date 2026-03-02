// using Bacera.Gateway.Auth;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class WalletTests : Startup
// {
//     private readonly IAccountingService _svc;
//
//     public WalletTests()
//     {
//         _svc = new AccountingService(TenantDbContext, AuthDbContext);
//     }
//
//     [Fact]
//     public async Task GetWalletTest()
//     {
//         var party = await FakePartyForClient();
//         var wallet = await _svc.WalletGetOrCreateAsync(party.Id, CurrencyTypes.USD);
//         wallet.ShouldNotBeNull();
//         wallet.Id.ShouldBeGreaterThan(0);
//
//         var walletExists = await _svc.WalletGetForPartyAsync(wallet.Id, party.Id);
//         walletExists.Id.ShouldBe(wallet.Id);
//
//         var partyWalletExists = await _svc.WalletGetForPartyAsync(wallet.Id, wallet.PartyId);
//         partyWalletExists.Id.ShouldBe(wallet.Id);
//     }
//
//     // [Fact]
//     // public async Task ChangeBalanceTest()
//     // {
//     //     var party = await FakeParty();
//     //     var user = FakeUser(party.Id);
//     //     var wallet = await _svc.WalletGetOrCreateAsync(party.Id, CurrencyTypes.CNY);
//     //     wallet.ShouldNotBeNull();
//     //     wallet.Balance.ShouldBe(0);
//     //
//     //     var deposit = await _svc.DepositCreateAsync(user.PartyId, 100, (CurrencyTypes)wallet.CurrencyId);
//     //     deposit.Id.ShouldBeGreaterThan(0);
//     //
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //
//     //     wallet.ShouldNotBeNull();
//     //     wallet.Balance.ShouldBe(100);
//     //     var transitions = await tenantDbContext.WalletTransactions
//     //         .Where(x => x.WalletId == wallet.Id)
//     //         .ToListAsync();
//     //     transitions.Count.ShouldBe(1);
//     //
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //     wallet.Balance.ShouldBe(200);
//     //
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //     wallet.Balance.ShouldBe(300);
//     //
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //     wallet.Balance.ShouldBe(400);
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //     wallet.Balance.ShouldBe(500);
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //     wallet.Balance.ShouldBe(600);
//     //     wallet = await _svc.WalletChangeBalanceAsync(party.Id, deposit.MatterId, 100, (CurrencyTypes)deposit.CurrencyId);
//     //     wallet.Balance.ShouldBe(700);
//     // }
// }

