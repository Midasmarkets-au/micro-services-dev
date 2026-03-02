// using Bogus;
// using Microsoft.EntityFrameworkCore;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class DepositTests
// {
//     private readonly Party _party;
//     private readonly Faker _faker;
//     private readonly Party _operator;
//     private readonly TenantDbContext _ctx;
//     private readonly IAccountingService _svc;
//
//     public DepositTests()
//     {
//         _ctx = Startup.CreateTenantDbContext();
//         _svc = new AccountingService(_ctx);
//         _ctx.SeedStateMachineAsync().Wait();
//         _party = _ctx.FakePartyForClient().Result;
//         _operator = _ctx.FakeParty(UserRoleTypes.TenantAdmin).Result;
//         _faker = new Faker();
//     }
//
//     [Fact]
//     public async Task DepositCreateTest()
//     {
//         const long amount = 1999L;
//         var deposit = await _svc.DepositCreateAsync(_party.Id, FundTypes.Fund,
//             CurrencyTypes.USD,
//             amount, (long)PaymentPlatformTypes.Wire, _party.Id);
//         deposit.Id.ShouldBeGreaterThan(0);
//         deposit.IdNavigation.StateId.ShouldBe((int)StateTypes.DepositCreated);
//         var log = await _ctx.Activities.AnyAsync(x => x.MatterId == deposit.Id);
//         log.ShouldBeTrue();
//     }
//
//     [Fact]
//     public async Task FullProcessTest()
//     {
//         var deposit = await DepositCreateTest();
//         var payment = await GetPayment(deposit);
//         var executedPaymentResult = await ExecutePayment(payment);
//         var completedPaymentResult = await CompletePayment(payment);
//         var isPaymentCompleted = await DepositCompletePayment(deposit);
//         var depositAfterPayment = await GetDepositAfterPayment(deposit);
//         await ApproveDepositByTenant(depositAfterPayment);
//         await CompleteDeposit(depositAfterPayment);
//     }
//
//     private async Task<Tuple<bool, Payment>> ExecutePayment(Payment payment)
//     {
//         const long amount = 1999L;
//         var result = await _svc.PaymentExecuteAsync(payment.Id, (long)PaymentPlatformTypes.Wire,
//             "token=randomString");
//         result.Item1.ShouldBeTrue();
//         result.Item2.Id.ShouldBe(payment.Id);
//         result.Item2.Amount.ShouldBe(amount);
//         result.Item2.Status.ShouldBe((short)PaymentStatusTypes.Executing);
//         return result;
//     }
//
//     private async Task<int> DepositCompletePayment(Deposit deposit)
//     {
//         var isPaymentCompleted = await _svc.DepositTryCompletePaymentAsync(deposit.Id, _operator.Id);
//         isPaymentCompleted.ShouldBe(1);
//         return isPaymentCompleted;
//     }
//
//     private async Task<(int, Payment)> CompletePayment(Payment payment)
//     {
//         var (status, paymentResult) = await _svc.PaymentCompleteAsync(payment.Id);
//         status.ShouldBe(1);
//         paymentResult.Id.ShouldBe(payment.Id);
//         paymentResult.Amount.ShouldBe(payment.Amount);
//         paymentResult.Status.ShouldBe((short)PaymentStatusTypes.Completed);
//         return (status, paymentResult);
//     }
//
//     private async Task<Deposit> GetDepositAfterPayment(Deposit deposit)
//     {
//         var depositAfterPayment = await _ctx.Deposits
//             .Include(x => x.Payment)
//             .Include(x => x.IdNavigation)
//             .FirstOrDefaultAsync(x => x.Id == deposit.Id);
//         depositAfterPayment.ShouldNotBeNull();
//         depositAfterPayment.Payment.ShouldNotBeNull();
//         depositAfterPayment.IdNavigation.StateId.ShouldBe((int)StateTypes.DepositPaymentCompleted);
//         return depositAfterPayment;
//     }
//
//     private async Task<Payment> GetPayment(Deposit deposit)
//     {
//         var payment = await _ctx.Payments
//             .Include(x => x.Deposits)
//             .Where(x => x.Status == (short)PaymentStatusTypes.Pending)
//             .Where(x => x.Deposits.Any(d => d.Id == deposit.Id))
//             .FirstOrDefaultAsync();
//
//         payment.ShouldNotBeNull();
//         payment.Id.ShouldBeGreaterThan(0);
//         return payment;
//     }
//
//     public async Task<bool> ApproveDepositByTenant(Deposit deposit)
//     {
//         var transitResult = await _svc.DepositApproveByTenantAsync(deposit, _operator.Id);
//         transitResult.ShouldBeTrue();
//         var matter = await _ctx.Matters.FirstOrDefaultAsync(x => x.Id == deposit.Id);
//         matter.ShouldNotBeNull();
//         matter.StateId.ShouldBe((int)StateTypes.DepositTenantApproved);
//         return transitResult;
//     }
//
//     private async Task<bool> CompleteDeposit(Deposit deposit)
//     {
//         var transitResult = await _svc.DepositCompleteAsync(deposit, _operator.Id);
//         transitResult.ShouldBeTrue();
//         var matter = await _ctx.Matters.FirstOrDefaultAsync(x => x.Id == deposit.Id);
//         matter.ShouldNotBeNull();
//         matter.StateId.ShouldBe((int)StateTypes.DepositCompleted);
//         return transitResult;
//     }
//
//     [Fact(Skip = "Only for filling data")]
//     public async Task FakeDepositTest()
//     {
//         var partyId = 100;
//         var ids = new List<long>();
//         for (var i = 0; i < 1000; i++)
//         {
//             var deposit = await _svc.DepositCreateAsync(partyId, FundTypes.Fund,
//                 CurrencyTypes.USD,
//                 _faker.Random.Long(100, 9802317), (long)PaymentPlatformTypes.Wire, partyId);
//
//             ids.Add(deposit.Id);
//         }
//
//         var deposits = await _ctx.Deposits
//             .Include(x => x.IdNavigation)
//             .Where(x => ids.Contains(x.Id))
//             .ToListAsync();
//         foreach (var deposit in deposits)
//         {
//             deposit.IdNavigation.StatedOn = _faker.Date.Between(DateTime.UtcNow.AddDays(-45), DateTime.UtcNow);
//             deposit.IdNavigation.StateId = (int)StateTypes.DepositCompleted;
//         }
//
//         _ctx.Deposits.UpdateRange(deposits);
//         await _ctx.SaveChangesAsync();
//     }
// }