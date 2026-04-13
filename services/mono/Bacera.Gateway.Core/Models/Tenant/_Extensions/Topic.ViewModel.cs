namespace Bacera.Gateway;

using M = Topic;

partial class Topic
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }
        public string HashId => HashEncode(Id);
        public short Type { get; set; }
        public string Title { get; set; } = null!;
        public short Category { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public Dictionary<string, TopicContent.ResponseModel> Contents { get; set; } = new();
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

public static class TopicViewModelExtension
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query, string? language = null)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Type = x.Type,
            Title = x.Title,
            Category = x.Category,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            Contents = language == null
                ? x.TopicContents
                    .ToLanguageDictionary()
                : x.TopicContents
                    .Where(l => l.Language.ToUpper().Equals(language.ToUpper()))
                    .ToLanguageDictionary(),
            UpdatedOn = x.UpdatedOn,
            CreatedOn = x.CreatedOn
        });
}