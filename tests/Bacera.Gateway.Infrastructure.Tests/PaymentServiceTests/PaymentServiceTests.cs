using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Models;
using Bacera.Gateway.Vendor.Help2Pay;
using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bacera.Gateway.Vendor.Poli.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Text;


namespace Bacera.Gateway.Infrastructure.Tests.PaymentServiceTests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class PaymentServiceTests : Startup
{
    private readonly Party _party;
    private readonly IPaymentProxyService _paymentSvc;

    public PaymentServiceTests()
    {
        _party = FakePartyForClient("Sender Client").Result;
        FakePaymentService().Wait();
        var mockLoggerFactory = new Mock<LoggerFactory>();
        var services = new IPaymentGatewayService[]
            { new ManualPaymentService(), new WirePaymentService()};
        // _paymentSvc = new PaymentProxyService(TenantDbContext, services, mockLoggerFactory.Object);
    }

    [Fact]
    public async Task ManualPaymentTest()
    {
        // Arrange
        var model = new ManualPaymentRequestModel { OperatorPartyId = _party.Id, Comment = "Unit Test" };
        var payment = await FakePaymentAsync(PaymentPlatformTypes.Manual);

        var result = await _paymentSvc.ProcessPaymentAsync(payment.Id, model);
        result.IsSuccess.ShouldBeTrue();

        payment = await TenantDbContext.Payments
            .Where(x => x.Status == (short)PaymentStatusTypes.Executing)
            .FirstOrDefaultAsync(x => x.Id == payment.Id);
        payment.ShouldNotBeNull();

        result = await _paymentSvc.ValidatePaymentAsync(payment.Id, model);
        result.IsSuccess.ShouldBeTrue();

        payment = await TenantDbContext.Payments
            .Where(x => x.Status == (short)PaymentStatusTypes.Completed)
            .FirstOrDefaultAsync(x => x.Id == payment.Id);
        payment.ShouldNotBeNull();
    }

    [Fact]
    public async Task WirePaymentTest()
    {
        // Arrange
        var model = WirePaymentRequestModel.Build("routing number", "account number", "account Name");
        var payment = await FakePaymentAsync(PaymentPlatformTypes.Wire);

        var result = await _paymentSvc.ProcessPaymentAsync(payment.Id, model);
        result.IsSuccess.ShouldBeTrue();

        payment = await TenantDbContext.Payments
            .Where(x => x.Status == (short)PaymentStatusTypes.Executing)
            .FirstOrDefaultAsync(x => x.Id == payment.Id);
        payment.ShouldNotBeNull();

        result = await _paymentSvc.ValidatePaymentAsync(payment.Id, model);
        result.IsSuccess.ShouldBeTrue();

        payment = await TenantDbContext.Payments
            .Where(x => x.Status == (short)PaymentStatusTypes.Completed)
            .FirstOrDefaultAsync(x => x.Id == payment.Id);
        payment.ShouldNotBeNull();
    }

    [Fact]
    public async Task Help2PayPaymentTest()
    {
        // Arrange
        // var model = Help2PayRequestModel.Build("Code", "customer", Guid.NewGuid().ToString()[..10],
        //     "BOA", CurrencyTypes.USD, 10000, "http://localhost:5000/api/v1/payments/validate", "http://localhost:5000/api/v1/payments/notify"
        // );
        // var payment = await FakePaymentAsync(PaymentPlatformTypes.Help2Pay);
        //
        // var result = await _paymentSvc.ProcessPaymentAsync(payment.Id, model);
        // result.IsSuccess.ShouldBeTrue();
        //
        // payment = await TenantDbContext.Payments
        //     .Where(x => x.Status == (short)PaymentStatusTypes.Executing)
        //     .FirstOrDefaultAsync(x => x.Id == payment.Id);
        // payment.ShouldNotBeNull();
        //
        // result = await _paymentSvc.ValidatePaymentAsync(payment.Id, model);
        // result.IsSuccess.ShouldBeTrue();
        //
        // payment = await TenantDbContext.Payments
        //     .Where(x => x.Status == (short)PaymentStatusTypes.Completed)
        //     .FirstOrDefaultAsync(x => x.Id == payment.Id);
        // payment.ShouldNotBeNull();
    }

    [Fact]
    public async Task PoliPaymentTest()
    {
        var payment = await FakePaymentAsync(PaymentPlatformTypes.Poli, 12);
        payment.ShouldNotBeNull();
        payment.IsEmpty().ShouldBeFalse();

        var options = new PoliOptions
        {
            MerchantCode = "Test",
            SecurityCode = "Test",
        };

        // Arrange
        var model = PoliRequestModel.Build(0.12m, payment.Number, options.FrontUri,
            options.CallBackUri, "https://pro.t.api.mybcr.dev/poli/success/failure",
            "https://pro.t.api.mybcr.dev/poli/success/cancellation");

        var result = await _paymentSvc.ProcessPaymentAsync(payment.Id, model);
        result.ShouldNotBeNull();
        result.Response.ShouldNotBeNull();
    }

    private async Task<Payment> FakePaymentAsync(PaymentPlatformTypes platformTypes, long amount = 10000)
    {
        var gateway = TenantDbContext.PaymentServices
            .Where(x => x.IsActivated == 1)
            .Where(x => x.CanDeposit == 1)
            .Where(x => x.CanWithdraw == 1)
            .FirstOrDefault(x => x.Platform == (short)platformTypes);
        gateway.ShouldNotBeNull();

        var payment = Payment.Build(_party.Id, LedgerSideTypes.Credit, gateway.Id, amount);
        await TenantDbContext.Payments.AddAsync(payment);
        await TenantDbContext.SaveChangesAsync();
        payment.Id.ShouldBeGreaterThan(0);
        return payment;
    }

    private async Task FakePaymentService()
    {
        var paymentGateways = new List<PaymentService>()
        {
            new()
            {
                Name = "Manually", Platform = (short)PaymentPlatformTypes.Manual,
                Configuration = "", Description = "   ",
                CanDeposit = 1, CanWithdraw = 1, IsActivated = 1
            },
            new()
            {
                Name = "Wire", Platform = (short)PaymentPlatformTypes.Wire, CanDeposit = 1,
                Configuration = "", Description = "   ",
                CanWithdraw = 1, IsActivated = 1
            },
            new()
            {
                Name = "Help2Pay", Platform = (short)PaymentPlatformTypes.Help2Pay, CanDeposit = 1,
                Configuration = "", Description = "   ",
                CanWithdraw = 1, IsActivated = 1
            },
            new()
            {
                Name = "Poli", Platform = (short)PaymentPlatformTypes.Poli, CanDeposit = 1,
                Configuration = "", Description = "   ",
                CanWithdraw = 1, IsActivated = 1
            },
        };
        await TenantDbContext.PaymentServices.AddRangeAsync(paymentGateways);
        await TenantDbContext.SaveChangesAsync();
    }
}