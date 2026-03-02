using Bacera.Gateway.Vendor.UniotcPay;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class UniotcPayTests
{
    private readonly UniotcPayOptions _options;

    public UniotcPayTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        _options = configuration.GetSection("UniotcPay").Get<UniotcPayOptions>() ?? new UniotcPayOptions();
        _options.ShouldNotBeNull();
        _options.IsValid().ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_ShouldReturnSuccess()
    {
        var request = new UniotcPayRequestModel
        {
            Amount = 1005,
            PaymentNumber = "PM-20230810-A1",
            PaymentId = 11223344,
        };
        request.ApplyOptions(_options);
        var service = new UniotcPayService(_options);
        var result = await service.MakePaymentAsync(request);
        result.IsSuccess.ShouldBeTrue();
    }
}