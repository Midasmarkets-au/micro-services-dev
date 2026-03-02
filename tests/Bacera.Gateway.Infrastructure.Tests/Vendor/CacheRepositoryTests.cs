// using Bacera.Gateway.Services;
// using Microsoft.Extensions.DependencyInjection;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests.Vendor;
//
// public class CacheRepositoryTests : Startup
// {
//     private readonly ICacheRepository _cacheRepo;
//
//     public CacheRepositoryTests()
//     {
//         _cacheRepo = ServiceProvider.GetRequiredService<ICacheRepository>();
//     }
//
//     [Fact]
//     public async Task GetStringAsync_ShouldReturnNull_WhenKeyDoesNotExist()
//     {
//         // Arrange
//         var key = Faker.Random.String2(10);
//
//         // Act
//         var result = await _cacheRepo.GetStringAsync(key);
//
//         // Assert
//         result.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task GetKeysAsync_ShouldReturnNotNull_WhenSearchExistsKey()
//     {
//         // Arrange
//         // var key = Faker.Random.String2(10);
//         // var key2 = key + "_2";
//         //
//         // // Act
//         // await _cacheRepo.SetStringAsync(key, Faker.Random.String2(10));
//         // await _cacheRepo.SetStringAsync(key2, Faker.Random.String2(10));
//         //
//         // // Assert
//         // (await _cacheRepo.GetStringAsync(key)).ShouldNotBeNullOrEmpty();
//         // (await _cacheRepo.GetStringAsync(key2)).ShouldNotBeNullOrEmpty();
//         //
//         // var keyList = new List<string>();
//         //
//         // await foreach (var item in _cacheRepo.GetKeysAsync(key + "*"))
//         // {
//         //     keyList.Add(item);
//         // }
//         //
//         // keyList.Count.ShouldBe(2);
//         //
//         // await _cacheRepo.RemoveWithWildCardAsync(key + "*");
//         // var deletedKeys = new List<string>();
//         //
//         // await foreach (var item in _cacheRepo.GetKeysAsync(key + "*"))
//         // {
//         //     deletedKeys.Add(item);
//         // }
//         //
//         // deletedKeys.Count.ShouldBe(0);
//     }
//
//     [Fact]
//     public async Task SetObject_ShouldBeSuccess()
//     {
//         // Arrange
//         var key = Faker.Random.String2(10);
//
//         var user = new Auth.User
//         {
//             NativeName = key,
//         };
//         // Act
//         await _cacheRepo.SetAsync(key, user);
//         var cachedUser = await _cacheRepo.GetAsync<Auth.User>(key);
//         var notExistUser = await _cacheRepo.GetAsync<Auth.User>(Faker.Random.String2(10));
//
//         // Assert
//         cachedUser.ShouldNotBeNull();
//         cachedUser.NativeName.ShouldBe(user.NativeName);
//
//         notExistUser.ShouldBeNull();
//     }
// }

