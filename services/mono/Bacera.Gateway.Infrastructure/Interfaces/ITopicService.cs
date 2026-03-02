namespace Bacera.Gateway;

public interface ITopicService
{
    Task<Topic> GetAsync(int id);
    Task MoveToTrash(int id);
    Task<Topic> GetWithLanguageAsync(int id, string language);
    Task<Result<List<Topic.ResponseModel>, Topic.Criteria>> QueryAsync(Topic.Criteria criteria);
    Task<Topic> CreateAsync(Topic.CreateSpec spec);
    Task<TopicContent> CreateContentAsync(int topicId, TopicContent.Spec spec);
    Task<Topic> UpdateAsync(int id, Topic.UpdateSpec spec);
    Task<TopicContent> UpdateContentAsync(int contentId, TopicContent.Spec spec);
    Task DeleteAsync(int id);
    Task DeleteContentAsync(int contentId);
    Task<bool> ExistsAsync(int id);
    Task<List<string>> GetLanguagesAsync(int id);
    Task<Topic.ResponseModel> GetNewsAsync(int id, string? language = null);
    Task<Topic.ResponseModel> GetCalenderAsync(int id, string? language = null);
    Task<Topic.ResponseModel> GetNoticeAsync(int id, string? language = null);
    Task<Topic.ResponseModel> GetContentAsync(string title, string? language = null);
}