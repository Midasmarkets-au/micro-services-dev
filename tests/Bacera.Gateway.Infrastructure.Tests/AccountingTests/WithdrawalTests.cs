// using Microsoft.EntityFrameworkCore;
//
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class WithdrawalTests
// {
//     private readonly Party _party;
//     private readonly Party _operator;
//     private readonly TenantDbContext _ctx;
//     private readonly IAccountingService _svc;
//
//     public WithdrawalTests()
//     {
//         _ctx = Startup.CreateTenantDbContext();
//         _svc = new AccountingService(_ctx, Startup.CreateAuthDbContext());
//         _ctx.SeedStateMachineAsync().Wait();
//         _party = _ctx.FakePartyForClient().Result;
//         _operator = _ctx.FakeParty(UserRoleTypes.TenantAdmin).Result;
//     }
//
//     [Fact]
//     public async Task WithdrawCreateTest()
//     {
//         var wallet = await _svc.WalletGetOrCreateAsync(_party.Id, CurrencyTypes.USD);
//         wallet.IsEmpty().ShouldBeFalse();
//         var updatedWallet = await _ctx.Wallets.SingleAsync(x => x.Id == wallet.Id);
//
//         updatedWallet.Balance.ShouldBe(0);
//         updatedWallet.Balance = 10000;
//         _ctx.Wallets.Update(updatedWallet);
//         await _ctx.SaveChangesAsync();
//
//         var withdraw = await _svc.WithdrawalCreateAsync(
//             (long)PaymentPlatformTypes.Wire,
//             wallet.PartyId, (FundTypes)wallet.FundType,
//             (CurrencyTypes)wallet.CurrencyId,
//             100,
//             _party.Id
//         );
//         withdraw.IsEmpty().ShouldBeFalse();
//     }
//
//     [Fact]
//     public async Task WithdrawOverAmountFailTest()
//     {
//         var wallet = await _svc.WalletGetOrCreateAsync(_party.Id, CurrencyTypes.USD);
//         var withdraw = await _svc.WithdrawalCreateAsync(
//             (long)PaymentPlatformTypes.Wire,
//             wallet.PartyId, (FundTypes)wallet.FundType,
//             (CurrencyTypes)wallet.CurrencyId,
//             wallet.Balance + 1,
//             (long)UserRoleTypes.System
//         );
//         withdraw.IsEmpty().ShouldBeTrue();
//     }
//
//     [Fact]
//     public async Task WithdrawApproveByTenantTest()
//     {
//         await WithdrawCreateTest();
//         var withdraw = await _ctx.Withdrawals
//             .Include(x => x.Payment)
//             .Where(x => x.IdNavigation.StateId == (int)StateTypes.WithdrawalCreated)
//             .Where(x => x.Payment.Status == (short)PaymentStatusTypes.Pending)
//             .FirstOrDefaultAsync();
//         withdraw.ShouldNotBeNull();
//         var result = await _svc.WithdrawalApproveByTenantAsync(withdraw, _operator.Id);
//         result.ShouldBeTrue();
//     }
//
//     [Fact]
//     public async Task WithdrawTryCompletePaymentTest()
//     {
//         await WithdrawApproveByTenantTest();
//         var withdraw = await _ctx.Withdrawals
//             .Include(x => x.Payment)
//             .Where(x => x.IdNavigation.StateId == (int)StateTypes.WithdrawalTenantApproved)
//             .Where(x => x.Payment.Status == (short)PaymentStatusTypes.Pending)
//             .FirstOrDefaultAsync();
//         withdraw.ShouldNotBeNull();
//         var result = await _svc.WithdrawalTryCompletePaymentAsync(withdraw.Id, _operator.Id);
//         result.ShouldBeFalse();
//
//         withdraw.Payment.UpdatedOn = DateTime.UtcNow;
//         _ctx.Payments.Update(withdraw.Payment);
//         await _ctx.SaveChangesAsync();
//
//         await _svc.PaymentExecuteAsync(withdraw.PaymentId);
//         await _svc.PaymentCompleteAsync(withdraw.PaymentId);
//
//         result = await _svc.WithdrawalTryCompletePaymentAsync(withdraw.Id, _operator.Id);
//         result.ShouldBeTrue();
//     }
//
//     [Fact]
//     public async Task WithdrawCompleteTest()
//     {
//         await WithdrawTryCompletePaymentTest();
//         var withdraw = await _ctx.Withdrawals
//             .Where(x => x.IdNavigation.StateId == (int)StateTypes.WithdrawalPaymentCompleted)
//             .OrderByDescending(x => x.Id)
//             .FirstOrDefaultAsync();
//         withdraw.ShouldNotBeNull();
//         var result = await _svc.WithdrawalCompleteAsync(withdraw, _operator.Id);
//         result.ShouldBeTrue();
//     }
// }