using Bacera.Gateway.Vendor.BipiPay;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class BipiPayTests
{
    private readonly BipiPayOptions _options;

    public BipiPayTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        _options = configuration.GetSection("BipiPay").Get<BipiPayOptions>() ?? new BipiPayOptions();
        _options.ShouldNotBeNull();
        _options.IsValid().ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_USD_ShouldReturnSuccess()
    {
        var request = new BipiPayRequestModel
        {
            PartyId = 100,
            Amount = 1001.05m,
            UsdAmount = 7251.00m,
            PaymentNumber = "PM-20230809-010",
            RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/bipi-pay"
        };
        request.ApplyOptions(_options);
        var service = new BipiPayService(_options);
        var result = await service.MakePaymentAsync(request);
        result.IsSuccess.ShouldBeTrue();
    }

    // [Fact]
    // public void Sha265HashTest()
    // {
    //     var str =
    //         "appid=10000&amount=100.00&out_trade_no=PM-333&pay_id=8201&return_type=pc&show_type=static&success_url=https://localhost:5001/api/v1/payment/callback/uniotc/success&version=v2.0&key=1234567890";
    //     var hashValue = Utils.SignSha265HashWithPkcs8PrivateKey(str, _options.PrivateKey);
    //     hashValue.ShouldNotBeNull();
    //
    //     var verified = Utils.VerifySignatureForSha265Hash(str, hashValue, _options.PublicKey);
    // }
}