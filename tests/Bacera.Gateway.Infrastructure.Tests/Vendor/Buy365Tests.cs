using Bacera.Gateway.Vendor.Buy365;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class Buy365Tests
{
    private readonly Buy365Options _options;

    public Buy365Tests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        _options = configuration.GetSection("Buy365").Get<Buy365Options>() ?? new Buy365Options();
        _options.ShouldNotBeNull();
        _options.IsValid().ShouldBeTrue();
    }

    // [Fact]
    // public async Task OnMakePaymentAsync_USD_ShouldReturnSuccess()
    // {
    //     var request = new Buy365RequestModel
    //     {
    //         Amount = 3005,
    //         PaymentNumber = "PM-20230912-B",
    //         AccountId = 59999,
    //         AccountName = "John Snow",
    //     };
    //     request.ApplyOptions(_options);
    //     var service = new Buy365Service(_options);
    //     var result = await service.MakePaymentAsync(request);
    //     result.IsSuccess.ShouldBeTrue();
    // }

    // [Fact]
    // public void Sha265HashTest()
    // {
    //     var str =
    //         "appid=10000&amount=100.00&out_trade_no=PM-333&pay_id=8201&return_type=pc&show_type=static&success_url=https://localhost:5001/api/v1/payment/callback/uniotc/success&version=v2.0&key=1234567890";
    //     var hashValue = Utils.SignSha265HashWithPkcs8PrivateKey(str, _options.PrivateKey);
    //     hashValue.ShouldNotBeNull();
    //
    //     Utils.VerifySignatureForSha265Hash(str, hashValue, _options.PublicKey);
    // }
}