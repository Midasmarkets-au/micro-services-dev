using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class TopicContent
{
    public TopicContent()
    {
        Title = string.Empty;
        Subtitle = string.Empty;
    }

    public sealed class Spec
    {
        public string Title { get; set; } = null!;
        public string Subtitle { get; set; } = string.Empty;

        [RegularExpression(LanguageTypes.AllLanguageRegEx)]
        [Required]
        public string Language { get; set; } = null!;

        [Required] public string Content { get; set; } = null!;
        public string Author { get; set; } = string.Empty;
    }

    public sealed class ResponseModel
    {
        public int Id { get; set; }
        public string Language { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Subtitle { get; set; } = null!;

        public DateTime UpdatedOn { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = null!;
    }

    public static bool TryParse(string json, out TopicContent source)
    {
        source = new TopicContent();
        try
        {
            source = JsonConvert.DeserializeObject<TopicContent>(json)!;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to parse TopicContent.Spec", e);
            return false;
        }
    }
}

public static class TopicContentExtension
{
    public static TopicContent Build(this TopicContent.Spec spec, int topicId)
        => new()
        {
            TopicId = topicId,
            Title = spec.Title,
            Subtitle = spec.Subtitle,
            Content = spec.Content,
            Language = spec.Language,
            Author = string.Empty,
        };

    public static Dictionary<string, TopicContent.ResponseModel> ToLanguageDictionary(this IEnumerable<TopicContent> me)
        => me
            .Select(x => new TopicContent.ResponseModel
            {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Subtitle,
                UpdatedOn = x.UpdatedOn,
                Content = x.Content,
                Language = x.Language,
                Author = x.Author,
            })
            .AsEnumerable()
            .ToDictionary(x => x.Language, x => x);
}