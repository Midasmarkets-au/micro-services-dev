using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class EventShopItem : IHasHashId
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.EventShopItem, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.EventShopItem]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public class BaseModel
    {
        public long Point { get; set; }
        public EventShopItemStatusTypes Status { get; set; }
        public EventShopItemCategoryTypes Category { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }


        public EventShopItemTypes Type { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ConfigurationJson { get; set; } = "{}";

        public dynamic Configuration => Utils.JsonDeserializeDynamic(ConfigurationJson);
    }

    public sealed class TenantPageModel : BaseModel
    {
        public long Id { get; set; }
        public long EventId { get; set; }

        public string Title { get; set; } = "";
        public string Name { get; set; } = "";

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string AccessSitesJson { get; set; } = "[]";

        public int[] AccessSites => JsonConvert.DeserializeObject<int[]>(AccessSitesJson) ?? Array.Empty<int>();
    }

    public sealed class TenantDetailModel : BaseModel
    {
        public long Id { get; set; }
        public long EventId { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string AccessRolesJson { get; set; } = "[]";

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string AccessSitesJson { get; set; } = "[]";

        public string[] AccessRoles =>
            JsonConvert.DeserializeObject<string[]>(AccessRolesJson) ?? Array.Empty<string>();

        public int[] AccessSites => JsonConvert.DeserializeObject<int[]>(AccessSitesJson) ?? Array.Empty<int>();

        public IList<TenantLanguageModel> LanguageList { get; set; } = new List<TenantLanguageModel>();
    }

    public class ClientPageModel : BaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public string HashId => HashEncode(Id);

        public string Title { get; set; } = "";
        public string Name { get; set; } = "";

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ImagesJson { get; set; } = "[]";


        public List<string> Images => Utils.JsonDeserializeObjectWithDefault<List<string>>(ImagesJson);
    }

    public sealed class ClientDetailModel : ClientPageModel
    {
        public string? Description { get; set; } = "";
    }

    public class TenantLanguageModel
    {
        public string Language { get; set; } = "";
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ImagesJsonArray { get; set; } = "[]";

        public List<string> Images =>
            JsonConvert.DeserializeObject<List<string>>(ImagesJsonArray) ?? new List<string>();
    }
}

public static class EventShopItemViewModelExtensions
{
    public static IQueryable<EventShopItem.TenantPageModel> ToTenantPageModel(this IQueryable<EventShopItem> q,
        string lang)
        => q.Select(x => new EventShopItem.TenantPageModel
        {
            Id = x.Id,
            EventId = x.EventId,
            Point = x.Point,
            Status = (EventShopItemStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Title = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Title
                : "",
            Name = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Name
                : "",
            Type = (EventShopItemTypes)x.Type,
            Category = (EventShopItemCategoryTypes)x.Category,
            ConfigurationJson = x.Configuration,
            AccessSitesJson = x.AccessSites,
        });

    public static IQueryable<EventShopItem.TenantDetailModel> ToTenantDetailModel(this IQueryable<EventShopItem> q)
        => q.Select(x => new EventShopItem.TenantDetailModel
        {
            Id = x.Id,
            EventId = x.EventId,
            Point = x.Point,
            Status = (EventShopItemStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Type = (EventShopItemTypes)x.Type,
            Category = (EventShopItemCategoryTypes)x.Category,
            AccessRolesJson = x.AccessRoles,
            AccessSitesJson = x.AccessSites,
            ConfigurationJson = x.Configuration,
            LanguageList = x.EventShopItemLanguages.Select(y => new EventShopItem.TenantLanguageModel
            {
                Language = y.Language,
                Name = y.Name,
                Title = y.Title,
                Description = y.Description,
                ImagesJsonArray = y.Images
            }).ToList()
        });

    public static IQueryable<EventShopItem.ClientPageModel> ToClientPageModel(this IQueryable<EventShopItem> q,
        string lang)
        => q.Select(x => new EventShopItem.ClientPageModel
        {
            Id = x.Id,
            Point = x.Point,
            Status = (EventShopItemStatusTypes)x.Status,
            ConfigurationJson = x.Configuration,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Title = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Title
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Title,
            Name = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Name
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Name,
            ImagesJson = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Images
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Images,
            Type = (EventShopItemTypes)x.Type,
            Category = (EventShopItemCategoryTypes)x.Category,
        });

    public static IQueryable<EventShopItem.ClientDetailModel> ToClientDetailModel(this IQueryable<EventShopItem> q,
        string lang)
        => q.Select(x => new EventShopItem.ClientDetailModel
        {
            Id = x.Id,
            Point = x.Point,
            Status = (EventShopItemStatusTypes)x.Status,
            ConfigurationJson = x.Configuration,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Title = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Title
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Title,
            Name = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Name
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Name,
            Description = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Description
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Description,
            ImagesJson = x.EventShopItemLanguages.Any(y => y.Language == lang)
                ? x.EventShopItemLanguages.First(y => y.Language == lang).Images
                : x.EventShopItemLanguages.First(y => y.Language == "en-us").Images,
            Type = (EventShopItemTypes)x.Type,
            Category = (EventShopItemCategoryTypes)x.Category,
        });
}