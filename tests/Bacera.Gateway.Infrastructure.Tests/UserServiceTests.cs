using Bacera.Gateway.Auth;
using Bacera.Gateway.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class UserServiceTests : Startup, IClassFixture<SharedTestFixture>
{
    private readonly UserService _userSvc;

    private readonly SharedTestFixture _sharedTestFixture;

    public UserServiceTests(SharedTestFixture sharedTestFixture)
    {
        _sharedTestFixture = sharedTestFixture;
        var opts = Options.Create(new MemoryDistributedCacheOptions());
        IDistributedCache cache = new MemoryDistributedCache(opts);
        _userSvc = new UserService(TenantDbContext, AuthDbContext, cache, TenancyResolver, new NullLoggerFactory());
    }

    // [Fact]
    // public async Task CreateUser_WithTags_ReturnSuccess()
    // {
    //     const string tagName = "UNIT TEST";
    //     var user = await _userSvc.CreateUserAsync(new User
    //     { Email = "test@gm.com", TenantId = 1, ConcurrencyStamp = Guid.NewGuid().ToString() });
    //     user.Id.ShouldBeGreaterThan(0);
    //     var claims = await AuthDbContext.UserClaims.Where(x => x.UserId == user.Id).ToListAsync();
    //     claims.Count.ShouldBe(2);
    //
    //     await _userSvc.AddUserTagAsync(user.Id, tagName);
    //
    //     var userTags = await GetUserTags(user.Id);
    //     userTags.ShouldNotBeNull();
    //     userTags.Count.ShouldBe(1);
    //
    //     await _userSvc.RemoveUserTagAsync(user.Id, tagName);
    //     userTags = await GetUserTags(user.Id);
    //     userTags.Any().ShouldBeFalse();
    // }
    //
    // [Fact]
    // public async Task CreateOrGetTagAsync_SameTagName_ReturnsSameTag()
    // {
    //     var tag = await _userSvc.CreateOrGetTagAsync("Unit Test1");
    //     var tag2 = await _userSvc.CreateOrGetTagAsync("Unit Test1");
    //
    //     tag.Id.ShouldBe(tag2.Id);
    // }
    //
    // [Fact]
    // public async Task CreateOrGetTagAsync_SameTagNameWithExtraSpaces_ReturnsSameTag()
    // {
    //     var tag = await _userSvc.CreateOrGetTagAsync("Unit Test2");
    //     var tag3 = await _userSvc.CreateOrGetTagAsync("  Unit Test2   ");
    //
    //     tag.Id.ShouldBe(tag3.Id);
    // }
    //
    // [Fact]
    // public async Task CreateOrGetTagAsync_DifferentCaseTagName_ReturnsDifferentTags()
    // {
    //     var tag = await _userSvc.CreateOrGetTagAsync("Unit Test3");
    //     var tag4 = await _userSvc.CreateOrGetTagAsync("UNIT TEST3");
    //
    //     tag.Id.ShouldBeLessThan(tag4.Id);
    // }
    //
    // [Fact]
    // public async Task CreateOrGetTagAsync_TagPropertiesAreValid()
    // {
    //     var tag = await _userSvc.CreateOrGetTagAsync("Unit Test4");
    //
    //     tag.ShouldNotBeNull();
    //     tag.IsEmpty().ShouldBeFalse();
    //     tag.Id.ShouldBeGreaterThan(0);
    // }
    //
    // [Fact]
    // public async Task QueryTags_Keywords_ReturnCountGreatThanOne()
    // {
    //     var result = await _userSvc.QueryTagsAsync(new Tag.Criteria());
    //     result.ShouldNotBeNull();
    //     result.Data.Count.ShouldBeGreaterThan(0);
    //
    //     result = await _userSvc.QueryTagsAsync(new Tag.Criteria { Keywords = UserTagTypes.Client.Name });
    //     result.ShouldNotBeNull();
    //     result.Data.Count.ShouldBe(1);
    // }

    // private async Task<List<Tag>> GetUserTags(long userId)
    //     => await AuthDbContext.Users
    //         .Include(x => x.Tags)
    //         .Where(x => x.Id == userId)
    //         .SelectMany(x => x.Tags).ToListAsync();
}