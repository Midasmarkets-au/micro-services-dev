// using Bogus;
//
// using Microsoft.EntityFrameworkCore;
//
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class PaymentTests
// {
//     private readonly Faker _faker;
//     private readonly TenantDbContext _ctx;
//     private readonly AuthDbContext _auCtx;
//     private readonly AccountingService _svc;
//
//
//     public PaymentTests()
//     {
//         _faker = new Faker();
//         _ctx = Startup.CreateTenantDbContext();
//         _auCtx = Startup.CreateAuthDbContext();
//         _svc = new AccountingService(_ctx, _auCtx);
//     }
//
//     [Fact]
//     public async Task CreatePaymentTest()
//     {
//         var party = await _ctx.FakePartyForClient();
//         var payment = Payment.Build(party.Id, LedgerSideTypes.Debit, (long)PaymentPlatformTypes.System, 1999);
//         _ctx.Payments.Add(payment);
//         await _ctx.SaveChangesAsync();
//         payment.Id.ShouldBeGreaterThan(0);
//         var supplement = Supplement.Build(SupplementTypes.Payment, payment.Id);
//         var data = Supplement.PaymentSupplement.FromJson("{}");
//         data.Reference = "123456789";
//         data.Token = "TOKEN";
//         supplement.Data = data.ToJson();
//         await _ctx.Supplements.AddAsync(supplement);
//         await _ctx.SaveChangesAsync();
//         supplement.Id.ShouldBeGreaterThan(0);
//     }
//
//     [Fact]
//     public async Task CancelTest()
//     {
//         await CreatePaymentTest();
//         var payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Pending)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//         var paymentId = payment.Id;
//         var (status, _) = await _svc.PaymentCancelAsync(payment.Id, "Cancel");
//         status.ShouldBe(1);
//
//         payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Cancelled)
//             .Where(x => x.Id == paymentId)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task ExecuteTest()
//     {
//         await CreatePaymentTest();
//         var payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Pending)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//         var paymentId = payment.Id;
//         var result = await _svc.PaymentExecuteAsync(payment.Id, 0, "Executing");
//         result.Item1.ShouldBeTrue();
//
//         payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Executing)
//             .Where(x => x.Id == paymentId)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task FailedTest()
//     {
//         await ExecuteTest();
//         var payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Executing)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//         var paymentId = payment.Id;
//         var (status, _) = await _svc.PaymentFailAsync(payment.Id, "failed");
//         status.ShouldBe(1);
//
//         payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Failed)
//             .Where(x => x.Id == paymentId)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task CompleteTest()
//     {
//         await ExecuteTest();
//         var payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Executing)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//         var paymentId = payment.Id;
//         var (status, _) = await _svc.PaymentCompleteAsync(payment.Id, "Completed");
//         status.ShouldBe(1);
//
//         payment = await _ctx.Payments.Where(x => x.Status == (short)PaymentStatusTypes.Completed)
//             .Where(x => x.Id == paymentId)
//             .FirstOrDefaultAsync();
//         payment.ShouldNotBeNull();
//     }
//
//     private async Task FakePaymentService()
//     {
//         var paymentGateways = new List<PaymentService>
//         {
//             new()
//             {
//                 Name = "Manually", Platform = (short)PaymentPlatformTypes.Manual,
//                 Configuration = "", Description = "   ",
//                 CanDeposit = 1, CanWithdraw = 1, IsActivated = 1
//             },
//             new()
//             {
//                 Name = "Manually2", Platform = (short)PaymentPlatformTypes.Manual,
//                 Configuration = "", Description = "   ",
//                 CanDeposit = 1, CanWithdraw = 1, IsActivated = 1
//             },
//             new()
//             {
//                 Name = "Wire", Platform = (short)PaymentPlatformTypes.Wire, CanDeposit = 1,
//                 Configuration = "", Description = "   ",
//                 CanWithdraw = 1, IsActivated = 1
//             },
//             new()
//             {
//                 Name = "Wire2", Platform = (short)PaymentPlatformTypes.Wire, CanDeposit = 1,
//                 Configuration = "", Description = "   ",
//                 CanWithdraw = 1, IsActivated = 1
//             },
//         };
//         await _ctx.PaymentServices.AddRangeAsync(paymentGateways);
//         await _ctx.SaveChangesAsync();
//     }
// }

