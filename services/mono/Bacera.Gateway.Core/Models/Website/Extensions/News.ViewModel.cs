using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class News
{
    public sealed class PublicModel
    {
        public string? Category { get; set; }

        public string Title { get; set; } = null!;

        public DateTime? PublishedDate { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string? DataString { get; set; }

        public dynamic Data => DataString != null ? Utils.JsonDeserializeDynamic(DataString) : "{}";

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string? LanguageString { get; set; }

        public string Language => LanguageTypes.ParseWebsiteLanguage(LanguageString ?? "en");

        public uint? Pid { get; set; }

        public string? Intro { get; set; }
    }
}

public static class NewsViewModelExtensions
{
    public static IQueryable<News.PublicModel> ToPublicModel(this IQueryable<News> query) => query.Select(e =>
        new News.PublicModel
        {
            Category = e.Category,
            Title = e.Title,
            PublishedDate = e.PublishedDate,
            DataString = e.Data,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            LanguageString = e.Language,
            Pid = e.Pid,
            Intro = e.Intro
        });
}