using Bacera.Gateway.Vendor.ChipPay;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class ChipPayTests
{
    private readonly ChipPayOptions _options;

    public ChipPayTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        _options = configuration.GetSection("ChipPay").Get<ChipPayOptions>() ?? new ChipPayOptions();
        _options.ShouldNotBeNull();
        _options.IsValid().ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_USD_ShouldReturnSuccess()
    {
        var request = new ChipPayRequestModel
        {
            Amount = 10005,
            PaymentNumber = "PM-20230809-010",
            RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/chip-pay"
        };
        request.ApplyOptions(_options);
        var service = new ChipPayService(_options);
        var result = await service.MakePaymentAsync(request);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_VND_ShouldReturnSuccess()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var options = configuration.GetSection("ChipPayVND").Get<ChipPayOptions>() ?? new ChipPayOptions();
        options.ShouldNotBeNull();
        options.IsValid().ShouldBeTrue();

        var request = new ChipPayRequestModel
        {
            CurrencyId = CurrencyTypes.VND,
            Amount = 10005,
            PaymentNumber = "PM-20230809-011",
            RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/chip-pay"
        };
        request.ApplyOptions(_options);
        var service = new ChipPayService(_options);
        var result = await service.MakePaymentAsync(request);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Sha265HashTest()
    {
        var str =
            "appid=10000&amount=100.00&out_trade_no=PM-333&pay_id=8201&return_type=pc&show_type=static&success_url=https://localhost:5001/api/v1/payment/callback/uniotc/success&version=v2.0&key=1234567890";
        var hashValue = Utils.SignSha265HashWithPkcs8PrivateKey(str, _options.PrivateKey);
        hashValue.ShouldNotBeNull();

        Utils.VerifySignatureForSha265Hash(str, hashValue, _options.PublicKey);
    }
}