using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Post
{
    public class PublicBaseModel
    {
        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public uint Id { get; set; }

        public string HashId => HashEncode(Id);
        public string Title { get; set; } = null!;

        public string? Subtitle { get; set; }

        public string Slug { get; set; } = null!;

        public string LanguageCode { get; set; } = null!;

        public string? Type { get; set; }

        public string Tag { get; set; } = "{}";

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? PublishTime { get; set; }

        public string? Category { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string? LanguagesJson { get; set; }

        public dynamic Languages => LanguagesJson != null ? Utils.JsonDeserializeDynamic(LanguagesJson) : "[]";
    }

    public sealed class PublicPageModel : PublicBaseModel
    {
        
    }

    public sealed class PublicDetailModel : PublicBaseModel
    {
        public string Body { get; set; } = null!;
        public string? Image { get; set; }
    }
}

public static class PostViewModelExtensions
{
    public static IQueryable<Post.PublicPageModel> ToPublicPageModel(this IQueryable<Post> query) => query.Select(e =>
        new Post.PublicPageModel
        {
            Id = e.Id,
            Title = e.Title,
            Subtitle = e.Subtitle,
            Slug = e.Slug,
            LanguageCode = e.LanguageCode,
            Type = e.Type,
            Tag = e.Tag,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            PublishTime = e.PublishTime,
            Category = e.Category,
            LanguagesJson = e.Languages
        });

    public static IQueryable<Post.PublicDetailModel> ToPublicDetailModel(this IQueryable<Post> query) => query.Select(
        e => new Post.PublicDetailModel
            {
                Id = e.Id,
                Title = e.Title,
                Subtitle = e.Subtitle,
                Slug = e.Slug,
                Body = e.Body,
                LanguageCode = e.LanguageCode,
                Type = e.Type,
                Tag = e.Tag,
                Image = e.Image,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                PublishTime = e.PublishTime,
                Category = e.Category,
                LanguagesJson = e.Languages
            });
}