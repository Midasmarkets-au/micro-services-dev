using Bacera.Gateway.Vendor.Long77Pay;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class Long77PayTests
{
    private readonly Long77PayOptions _options;

    public Long77PayTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        _options = configuration.GetSection("Long77Pay").Get<Long77PayOptions>() ?? new Long77PayOptions();
        _options.ShouldNotBeNull();
        _options.IsValid().ShouldBeTrue();
    }

    [Fact]
    public async Task OnMakePaymentAsync_VND_ShouldReturnSuccess()
    {
        var request = new Long77PayRequestModel
        {
            Amount = 100,
            PaymentNumber = "PM-20231003-010B",
            ReturnUrl = "http://demo.localhost:5000/api/v1/payment/callback/Long77-pay"
        };
        request.ApplyOptions(_options);
        var service = new Long77PayService(_options);
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