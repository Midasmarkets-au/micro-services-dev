using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class TopicServiceTests : Startup
{
    private readonly TopicService _svc;

    public TopicServiceTests()
    {
        _svc = new TopicService(TenantDbContext);
    }

    [Fact]
    public async Task CreateTopicTest()
    {
        // Arrange
        var item = new Topic.CreateSpec
        {
            Author = "Unit Test",
            Content = "Content For Unit Test " + DateTime.UtcNow.ToLongDateString(),
            Title = "Unit Test Title " + Guid.NewGuid(),
            Language = LanguageTypes.English,
            EffectiveFrom = DateTime.UtcNow.AddMonths(-1),
            EffectiveTo = DateTime.UtcNow.AddMonths(1),
            Type = TopicTypes.Calendar,
        };

        // Act
        var result = await _svc.CreateAsync(item);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);

        // Arrange
        var criteria = new Topic.Criteria { Type = TopicTypes.Calendar };

        // Act
        var data = await _svc.QueryAsync(criteria);

        // Assert
        data.ShouldNotBeNull();
        data.Data.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task AddContentTest()
    {
        await CreateTopicTest();
        var result = await _svc.QueryAsync(new Topic.Criteria());
        var item = result.Data.FirstOrDefault();
        item.ShouldNotBeNull();
        item.Contents.ShouldNotBeNull();

        var content = new TopicContent.Spec
        {
            Author = "Language author",
            Content = "Language content " + DateTime.UtcNow.ToLongDateString(),
            Language = LanguageTypes.Chinese,
            Title = "Language title " + Guid.NewGuid(),
        };

        var data = await _svc.CreateContentAsync(item.Id, content);
        data.ShouldNotBeNull();
        data.Id.ShouldBeGreaterThan(0);
    }
}