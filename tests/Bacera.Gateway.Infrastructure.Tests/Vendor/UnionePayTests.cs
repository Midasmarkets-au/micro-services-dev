using Bacera.Gateway.Vendor.UnionePay;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class UnionePayTests
{
    private readonly UnionePayOptions _options;

    public UnionePayTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        _options = configuration.GetSection("UnionePay").Get<UnionePayOptions>() ?? new UnionePayOptions();
        _options.ShouldNotBeNull();
        _options.IsValid().ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_CNY_ShouldReturnSuccess()
    {
        // var request = new UnionePayRequestModel
        // {
        //     Amount = 10005,
        //     PaymentNumber = "PM-20230910-010",
        //     RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/unione-pay"
        // };
        // request.ApplyOptions(_options);
        // var service = new UnionePayService(_options);
        // var result = await service.MakePaymentAsync(request);
        // result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_USDT_ShouldReturnSuccess()
    {
        // var configuration = new ConfigurationBuilder()
        //     .AddJsonFile("appsettings.json")
        //     .Build();
        // var options = configuration.GetSection("UnionePayUSDT").Get<UnionePayOptions>() ?? new UnionePayOptions();
        // options.ShouldNotBeNull();
        // options.IsValid().ShouldBeTrue();
        //
        // var request = new UnionePayRequestModel
        // {
        //     Amount = 1005,
        //     PaymentNumber = "PM-20230809-011A",
        //     RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/unione-pay"
        // };
        // request.ApplyOptions(_options);
        // var service = new UnionePayService(_options);
        // var result = await service.MakePaymentAsync(request);
        // result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Sha265HashTest()
    {
        var str =
            "appid=10000&amount=100.00&out_trade_no=PM-333&pay_id=8201&return_type=pc&show_type=static&success_url=https://localhost:5001/api/v1/payment/callback/uniotc/success&version=v2.0&key=1234567890";
        var hashValue = Utils.SignSha265HashWithPkcs8PrivateKey(str, _options.MerchantKey);
        hashValue.ShouldNotBeNull();

        Utils.VerifySignatureForSha265Hash(str, hashValue, _options.MerchantKey);
    }
}