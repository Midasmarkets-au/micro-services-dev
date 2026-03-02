using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = EconomicCalendar;

public partial class EconomicCalendar
{
    public sealed class PublicModel
    {
        public string Event { get; set; } = "";

        public DateTime Date { get; set; }

        public string? Country { get; set; }

        public decimal? Actual { get; set; }

        public decimal? Previous { get; set; }

        public decimal? Change { get; set; }

        public decimal? ChangePercentage { get; set; }

        public decimal? Estimate { get; set; }

        public string? Impact { get; set; }

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
    }
}

public static class EconomicCalendarViewModelExtensions
{
    public static IQueryable<M.PublicModel> ToPublicModel(this IQueryable<M> query) => query.Select(e =>
        new M.PublicModel
        {
            Event = e.Event,
            Date = e.Date,
            Country = e.Country,
            Actual = e.Actual,
            Previous = e.Previous,
            Change = e.Change,
            ChangePercentage = e.ChangePercentage,
            Estimate = e.Estimate,
            Impact = e.Impact,
            DataString = e.Data,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            LanguageString = e.Language
        });
}