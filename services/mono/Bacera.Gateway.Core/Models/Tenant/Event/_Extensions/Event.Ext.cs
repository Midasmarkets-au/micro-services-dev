using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Event
{
    public IList<string> GetAccessRoles() =>
        JsonConvert.DeserializeObject<IList<string>>(AccessRoles) ?? new List<string>();

    public Event FromUpdateSpec(UpdateSpec updateSpec)
    {
        ApplyStartOn = updateSpec.ApplyStartOn;
        ApplyEndOn = updateSpec.ApplyEndOn;
        StartOn = updateSpec.StartOn;
        EndOn = updateSpec.EndOn;
        AccessRoles = JsonConvert.SerializeObject(updateSpec.AccessRoles);
        AccessSites = JsonConvert.SerializeObject(updateSpec.AccessSites);
        Key = updateSpec.Key;
        UpdatedOn = DateTime.UtcNow;
        return this;
    }

    public class BaseModel
    {
        public DateTime ApplyStartOn { get; set; }
        public DateTime ApplyEndOn { get; set; }
        public DateTime StartOn { get; set; }
        public DateTime EndOn { get; set; }
        public string Key { get; set; } = "";
        public EventStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Language { get; set; } = "";
    }

    public class TenantPageModel : BaseModel
    {
        public long Id { get; set; }


        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string AccessRolesJson { get; set; } = "[]";

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string AccessSitesJson { get; set; } = "[]";
        

        public string[] AccessRoles =>
            JsonConvert.DeserializeObject<string[]>(AccessRolesJson) ?? [];

        public string[] AccessSites =>
            JsonConvert.DeserializeObject<string[]>(AccessSitesJson) ?? [];
    }

    public class ClientPageModel : BaseModel
    {
    }

    public class TenantDetailModel : BaseModel
    {
        public long Id { get; set; }
        public IEnumerable<TenantLanguageModel> Languages { get; set; } = new List<TenantLanguageModel>();
        public string? Description { get; set; } = "";


        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string AccessRolesJson { get; set; } = "[]";

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string AccessSitesJson { get; set; } = "[]";

        public string[] AccessRoles => JsonConvert.DeserializeObject<string[]>(AccessRolesJson) ?? [];
        public int[] AccessSites => JsonConvert.DeserializeObject<int[]>(AccessSitesJson) ?? [];
    }

    public class ClientDetailModel : BaseModel
    {
        public string? Description { get; set; } = "";
        public string? Term { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ImagesJsonDict { get; set; } = "";

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string InstructionJson { get; set; } = "";

        public Dictionary<string,string> Images =>
            JsonConvert.DeserializeObject<Dictionary<string,string>>(ImagesJsonDict) ?? new Dictionary<string,string>();

        public Instruction Instruction => JsonConvert.DeserializeObject<Instruction>(InstructionJson)!;
    }

    public class TenantLanguageModel
    {
        public string Language { get; set; } = "";
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Term { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ImagesJsonDict { get; set; } = "{}";

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string InstructionJson { get; set; } = "{}";

        public Dictionary<string, string> Images =>
            JsonConvert.DeserializeObject<Dictionary<string, string>>(ImagesJsonDict) ?? new Dictionary<string, string>();

        public Instruction Instruction => JsonConvert.DeserializeObject<Instruction>(InstructionJson)!;
    }
}

public static class EventExtensions
{
    public static IQueryable<Event.TenantPageModel> ToTenantPageModel(this IQueryable<Event> @event,
        string language)
        => @event.Select(x => new Event.TenantPageModel
        {
            Id = x.Id,
            ApplyStartOn = x.ApplyStartOn,
            ApplyEndOn = x.ApplyEndOn,
            StartOn = x.StartOn,
            EndOn = x.EndOn,
            AccessRolesJson = x.AccessRoles,
            AccessSitesJson = x.AccessSites,
            Name = x.EventLanguages.Any(y => y.Language == language)
                ? x.EventLanguages.First(y => y.Language == language).Name
                : "",
            Title = x.EventLanguages.Any(y => y.Language == language)
                ? x.EventLanguages.First(y => y.Language == language).Title
                : "",
            Language = language,
            Key = x.Key,
            Status = (EventStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<Event.TenantDetailModel> ToTenantDetailModel(this IQueryable<Event> @event)
        => @event.Select(x => new Event.TenantDetailModel
        {
            Id = x.Id,
            ApplyStartOn = x.ApplyStartOn,
            ApplyEndOn = x.ApplyEndOn,
            StartOn = x.StartOn,
            EndOn = x.EndOn,
            AccessRolesJson = x.AccessRoles,
            AccessSitesJson = x.AccessSites,
            Languages = x.EventLanguages.Select(y => new Event.TenantLanguageModel
            {
                Language = y.Language,
                Name = y.Name,
                Title = y.Title,
                Description = y.Description,
                ImagesJsonDict = y.Images,
                Term = y.Term,
                InstructionJson = y.Instruction
            }),
            Key = x.Key,
            Status = (EventStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
        });


    public static IQueryable<Event.ClientPageModel> ToClientPageModel(this IQueryable<Event> @event,
        string language)
        => @event.Select(x => new Event.ClientPageModel
        {
            ApplyStartOn = x.ApplyStartOn,
            ApplyEndOn = x.ApplyEndOn,
            StartOn = x.StartOn,
            EndOn = x.EndOn,
            Name = x.EventLanguages.Any(y => y.Language == language)
                ? x.EventLanguages.First(y => y.Language == language).Name
                : "",
            Title = x.EventLanguages.Any(y => y.Language == language)
                ? x.EventLanguages.First(y => y.Language == language).Title
                : "",
            Language = language,
            Key = x.Key,
            Status = (EventStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<Event.ClientDetailModel> ToClientDetailModel(this IQueryable<Event> @event,
        string language)
        => @event
            .Select(x => new
            {
                Event = x,
                Language = x.EventLanguages.Any(y => y.Language == language)
                    ? x.EventLanguages.First(y => y.Language == language)
                    : x.EventLanguages.First(y => y.Language == "en-us"),
                ImagesString = x.EventLanguages.Any(y => y.Language == language)
                    ? x.EventLanguages.First(y => y.Language == language).Images
                    : x.EventLanguages.First(y => y.Language == "en-us").Images,
            })
            .Select(x => new Event.ClientDetailModel
            {
                ApplyStartOn = x.Event.ApplyStartOn,
                ApplyEndOn = x.Event.ApplyEndOn,
                StartOn = x.Event.StartOn,
                EndOn = x.Event.EndOn,
                Key = x.Event.Key,
                Status = (EventStatusTypes)x.Event.Status,
                CreatedOn = x.Event.CreatedOn,
                UpdatedOn = x.Event.UpdatedOn,
                Language = x.Language.Language,
                Name = x.Language.Name,
                Title = x.Language.Title,
                Description = x.Language.Description,
                Term = x.Language.Term,
                ImagesJsonDict = x.ImagesString,
                InstructionJson = x.Language.Instruction
            });
}