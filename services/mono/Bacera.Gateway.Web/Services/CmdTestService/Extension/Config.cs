using System.Security.Cryptography;
using System.Text;
using System.Web;
using Bacera.Gateway.Central;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.DTO;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.PaymentAsia;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task AddNewConfig()
    {
        await Task.WhenAll(_serviceProvider.GetDbPool().GetTenantIds().Select(async tid =>
        {
            var obj = new Dictionary<string, int>()
            {
                { "travelTheWorld", 24 },
                { "chineseBrandSkinCare", 23 },
                { "H520", 21 },
                { "midAutumnFestival", 22 },
                { "bcrMerch", 0 },
                { "luxury", 1 },
                { "electronics", 2 },
                { "vehicle", 3 },
                { "homeAppliances", 4 },
                { "luxuryFurniture", 20 },
                { "luxuryFood", 5 },
                { "tobacco", 6 },
                { "watch", 7 },
                { "jewelry", 8 },
                { "giftCard", 9 },
                { "travelPackage", 10 },
                { "skinCare", 11 },
                { "culterySet", 12 },
                { "phone", 13 },
                { "furniture", 14 },
                { "clock", 15 },
                { "outdoor", 16 },
                { "bag", 17 },
                { "art", 18 },
                { "makeup", 19 }
            };


            using var scope = _serviceProvider.CreateTenantScope(tid);
            var tenantCtx = scope.ServiceProvider.GetTenantDbContext();
            var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();
            var maxId = await tenantCtx.Configurations.Where(x => x.Id < 10000).MaxAsync(x => x.Id);

            await cfgSvc.SetAsync<object>("Public", 0
                , ConfigKeys.EventShopItemCategoryKey
                , obj
                , id: maxId + 1);
        }));

        // var str = JsonConvert.SerializeObject(new
        // {
        //     siteId = 1,
        //     tenantId = 10000,
        //     ver = "1.0.2",
        //     android = "https://play.google.com/store/apps/details?id=com.bacera.bcr",
        //     ios = "https://apps.apple.com/us/app/bacera/id1581440000",
        //     apk = "https://img.thebcr.com/app/bcrappv1_0_2.apk",
        //     forceUpdate = false,
        //     welcome = new[]
        //     {
        //         // new
        //         // {
        //         //     title = "Welcome to Bacera",
        //         //     content =
        //         //         "Bacera is a leading financial services provider in the Asia-Pacific region. We offer a wide range of financial products and services to individuals, businesses, and institutions. Our mission is to provide our clients with the best possible financial solutions to help them achieve their goals. We are committed to delivering exceptional customer service and building long-term relationships with our clients. We look forward to working with you and helping you achieve your financial goals.",
        //         //     pic = "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/images/welcome_1.png"
        //         // },
        //         // new
        //         // {
        //         //     title = "Welcome to Bacera",
        //         //     content =
        //         //         "Bacera is a leading financial services provider in the Asia-Pacific region. We offer a wide range of financial products and services to individuals, businesses, and institutions. Our mission is to provide our clients with the best possible financial solutions to help them achieve their goals. We are committed to delivering exceptional customer service and building long-term relationships with our clients. We look forward to working with you and helping you achieve your financial goals.",
        //         //     pic = "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/images/welcome_2.png"
        //         // },
        //         new
        //         {
        //             title = "Welcome to Bacera",
        //             content =
        //                 "Bacera is a leading financial services provider in the Asia-Pacific region. We offer a wide range of financial products and services to individuals, businesses, and institutions. Our mission is to provide our clients with the best possible financial solutions to help them achieve their goals. We are committed to delivering exceptional customer service and building long-term relationships with our clients. We look forward to working with you and helping you achieve your financial goals.",
        //             pic = "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/images/welcome_3.jpg"
        //         },
        //         new
        //         {
        //             title = "Welcome to Bacera",
        //             content =
        //                 "Bacera is a leading financial services provider in the Asia-Pacific region. We offer a wide range of financial products and services to individuals, businesses, and institutions. Our mission is to provide our clients with the best possible financial solutions to help them achieve their goals. We are committed to delivering exceptional customer service and building long-term relationships with our clients. We look forward to working with you and helping you achieve your financial goals.",
        //             pic = "https://bcrpropublic.s3.ap-southeast-1.amazonaws.com/images/welcome_4.jpg"
        //         },
        //     }
        // });
        // _centralDbContext.CentralConfigs.Add(new CentralConfig
        // {
        //     Category = "Public",
        //     Key = ConfigKeys.WelcomeInfo,
        //     Value = str,
        //     DataFormat = "json",
        //     Description = "Welcome Info",
        //     Name = "Welcome Info"
        // });
        //
        // await _centralDbContext.SaveChangesAsync();
    }
}