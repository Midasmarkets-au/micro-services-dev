// using Bacera.Gateway.Vendor.Amazon.Options;
// using Bacera.Gateway.Vendor.Amazon;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
// public class AwsStorageTests : Startup
// {
//     private readonly TenantDbContext _ctx;
//     private readonly AwsStorageService _svc;
//
//     public AwsStorageTests()
//     {
//         var options = ServiceProvider.GetRequiredService<IOptions<AwsS3Options>>();
//         _ctx = ServiceProvider.GetRequiredService<TenantDbContext>();
//         _svc = new AwsStorageService(options, _ctx);
//     }
//
//     [Fact]
//     public async Task UploadTest()
//     {
//         var file = new FileInfo("./Resources/bcr-logo.png");
//         using var stream = new MemoryStream(await File.ReadAllBytesAsync(file.FullName));
//         stream.ShouldNotBeNull();
//         stream.Length.ShouldBeGreaterThan(0);
//         stream.Position = 0;
//         var media = await _svc.UploadFileAsync(stream, "bcr-logo.png", file.Extension, "test", 0, "image/png",
//             tenancyResolver.Tenant.Id, 999);
//         media.ShouldNotBeNull();
//         media.Id.ShouldBeGreaterThan(0);
//     }
//
//     [Fact]
//     public async Task GetObjTest()
//     {
//         var item = await UploadTest();
//         item.ShouldNotBeNull();
//         var media = await _svc.GetObjectAsync(item.Id);
//         media.ShouldNotBeNull();
//         media.IsEmpty().ShouldBeFalse();
//         media.Stream.Length.ShouldBeGreaterThan(0);
//     }
// }