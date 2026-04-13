using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bacera.Gateway;

using M = Topic;

public class TopicService : ITopicService
{
    private readonly ILogger<TopicService> _logger;
    private readonly TenantDbContext _tenantDbContext;

    public TopicService(TenantDbContext tenantDbContext, ILogger<TopicService>? logger = null)
    {
        _tenantDbContext = tenantDbContext;
        _logger = logger ?? new NullLogger<TopicService>();
    }

    public async Task<M> GetAsync(int id) =>
        await _tenantDbContext.Topics
            .Include(x => x.TopicContents)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync() ?? new M();

    public async Task MoveToTrash(int id)
    {
        var item = await _tenantDbContext.Topics.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return;

        item.Type = (short)TopicTypes.Trash;
        item.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.Topics.Update(item);
        await _tenantDbContext.SaveChangesAsync();
    }

    public async Task<M> GetWithLanguageAsync(int id, string language) =>
        await _tenantDbContext.Topics
            .Include(x => x.TopicContents)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync() ?? new M();

    public async Task<Result<List<M.ResponseModel>, M.Criteria>> QueryAsync(M.Criteria criteria)
    {
        var items = await _tenantDbContext.Topics
            .PagedFilterBy<M, int>(criteria)
            .ToResponseModels(criteria.Language)
            .ToListAsync();
        return Result<List<M.ResponseModel>, M.Criteria>.Of(items, criteria);
    }

    public async Task<M> CreateAsync(M.CreateSpec spec)
    {
        var item = spec.Build();
        await _tenantDbContext.Topics.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
        _logger.LogInformation("Created topic {Id}", item.Id);
        return item;
    }

    public async Task<TopicContent> CreateContentAsync(int topicId, TopicContent.Spec spec)
    {
        var item = spec.Build(topicId);
        await _tenantDbContext.TopicContents.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
        _logger.LogInformation("Created content {Id} for topic {TopicId}", item.Id, topicId);
        return item;
    }

    public async Task<M> UpdateAsync(int id, M.UpdateSpec spec)
    {
        var item = await _tenantDbContext.Topics
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
        {
            return new M();
        }

        item.UpdatedOn = DateTime.UtcNow;
        item.Title = spec.Title ?? "Title";
        item.Type = (short)spec.Type;
        if (spec.Category != null) item.Category = (short)spec.Category;
        if (spec.EffectiveFrom != null) item.EffectiveFrom = spec.EffectiveFrom!.Value;
        if (spec.EffectiveTo != null) item.EffectiveTo = spec.EffectiveTo!.Value;

        _tenantDbContext.Topics.Update(item);
        await _tenantDbContext.SaveChangesAsync();

        _logger.LogInformation("Updated topic {Id}", item.Id);
        return item;
    }

    public async Task<TopicContent> UpdateContentAsync(int contentId, TopicContent.Spec spec)
    {
        var item = await _tenantDbContext.TopicContents
            .FirstOrDefaultAsync(x => x.Id == contentId);

        if (item == null)
        {
            return new TopicContent();
        }

        item.UpdatedOn = DateTime.UtcNow;
        item.Title = spec.Title;
        item.Subtitle = spec.Subtitle;
        item.Content = spec.Content;
        item.Language = spec.Language;
        _tenantDbContext.TopicContents.Update(item);
        await _tenantDbContext.SaveChangesAsync();
        _logger.LogInformation("Updated content {Id} for topic {TopicId}", item.Id, item.TopicId);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _tenantDbContext.Topics
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
        {
            return;
        }

        _tenantDbContext.Topics.Remove(item);
        await _tenantDbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted topic {Id}", item.Id);
    }

    public async Task DeleteContentAsync(int id)
    {
        var item = await _tenantDbContext.TopicContents
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
        {
            return;
        }

        _tenantDbContext.TopicContents.Remove(item);
        await _tenantDbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted content {Id} for topic {TopicId}", item.Id, item.TopicId);
    }

    public async Task<bool> ExistsAsync(int id)
        => await _tenantDbContext.Topics.AnyAsync(x => x.Id == id);

    public async Task<List<string>> GetLanguagesAsync(int id)
        => await _tenantDbContext.TopicContents
            .Where(x => x.TopicId == id)
            .Select(x => x.Language)
            .Distinct()
            .ToListAsync();

    public async Task<M.ResponseModel> GetNewsAsync(int id, string? language = null)
        => await _tenantDbContext.Topics
            .Where(x => x.Type == (short)TopicTypes.News)
            .Where(x => x.Id.Equals(id))
            .ToResponseModels(language)
            .FirstOrDefaultAsync() ?? new Topic.ResponseModel();

    public async Task<M.ResponseModel> GetNoticeAsync(int id, string? language = null)
        => await _tenantDbContext.Topics
            .Where(x => x.Type == (short)TopicTypes.Notice)
            .Where(x => x.Id.Equals(id))
            .Where(x => x.EffectiveFrom <= DateTime.UtcNow)
            .Where(x => x.EffectiveTo >= DateTime.UtcNow)
            .ToResponseModels(language)
            .FirstOrDefaultAsync() ?? new Topic.ResponseModel();

    public async Task<M.ResponseModel> GetContentAsync(string title, string? language = null)
        => await _tenantDbContext.Topics
            .Where(x => x.Type == (short)TopicTypes.Content)
            .Where(x => x.Title == title)
            .ToResponseModels(language)
            .FirstOrDefaultAsync() ?? new Topic.ResponseModel();

    public async Task<M.ResponseModel> GetCalenderAsync(int id, string? language = null)
        => await _tenantDbContext.Topics
            .Where(x => x.Type == (short)TopicTypes.Notice)
            .Where(x => x.Id.Equals(id))
            // .Where(x => x.EffectiveFrom <= DateTime.UtcNow)
            // .Where(x => x.EffectiveTo >= DateTime.UtcNow)
            .ToResponseModels(language)
            .FirstOrDefaultAsync() ?? new Topic.ResponseModel();
}