// using Bacera.Gateway.Vendor.GPay;
// using Microsoft.Extensions.Configuration;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests.Vendor;
//
// public class GPayTests
// {
//     private readonly GPayOptions _options;
//     private readonly IConfigurationRoot _configuration;
//
//     public GPayTests()
//     {
//          _configuration = new ConfigurationBuilder()
//             .AddJsonFile("appsettings.json")
//             .Build();
//         _options = _configuration.GetSection("GPay").Get<GPayOptions>() ?? new GPayOptions();
//         _options.ShouldNotBeNull();
//         _options.IsValid().ShouldBeTrue();
//     }
//
//     [Fact]
//     public async Task OnMakePaymentAsync_USDT_ShouldReturnSuccess()
//     {
//         var request = new GPayRequestModel
//         {
//             Amount = 100.03m,
//             Ip = "137.25.19.180",
//             PaymentNumber = "PM-20230815-013",
//             RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/gpay",
//         };
//         var service = new GPayService(_options);
//         var result = await service.MakePaymentAsync(request);
//         result.IsSuccess.ShouldBeTrue();
//     }
//     
//     [Fact]
//     public async Task OnMakePaymentAsync_Alipay_ShouldReturnSuccess()
//     {
//         var options = _configuration.GetSection("GPay").Get<GPayOptions>() ?? new GPayOptions();
//         options.ShouldNotBeNull();
//         options.IsValid().ShouldBeTrue();
//         
//         var request = new GPayRequestModel
//         {
//             Amount = 100.03m,
//             Ip = "137.25.19.180",
//             PaymentNumber = "PM-20231004-01A",
//             RedirectUrl = "http://demo.localhost:5000/api/v1/payment/callback/gpay",
//         };
//         var service = new GPayService(_options);
//         var result = await service.MakePaymentAsync(request);
//         result.IsSuccess.ShouldBeTrue();
//     }
// }

