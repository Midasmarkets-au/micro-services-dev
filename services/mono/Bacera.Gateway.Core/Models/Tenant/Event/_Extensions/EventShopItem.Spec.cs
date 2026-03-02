using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class EventShopItem
{
    public sealed class RewardConfiguration
    {
        public EventShopRewardTypes RewardType { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public long ValidPeriodInDays { get; set; }

        public List<string> BannerImages { get; set; } = new();

        public static bool TryParse(string configJson, out RewardConfiguration configuration)
        {
            try
            {
                configuration = JsonConvert.DeserializeObject<RewardConfiguration>(configJson)!;
                return true;
            }
            catch
            {
                configuration = new RewardConfiguration();
                return false;
            }
        }
    }


    public sealed class AddShopItemCategorySpec
    {
        public string Key { get; set; } = "";
        public Dictionary<string, string> Languages { get; set; } = new();

        /// <summary>
        /// 分类可用端: 0=None, 1=Web, 2=App, 3=Both (可选，默认为3)
        /// </summary>
        public int? AvailableOn { get; set; }
    }

    public sealed class ShopItemCategoryCriteria
    {
        public bool? Status { get; set; }
    }

    public class ShopItemCategoryData
    {
        public bool Status { get; set; }
        public long? Value { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 分类可用端: 0=None, 1=Web, 2=App, 3=Both (默认3)
        /// </summary>
        public int AvailableOn { get; set; } = 3;
    }

    public class CreateWithLanguageSpec
    {
        [Required] public long EventId { get; set; }
        public EventShopItemTypes Type { get; set; }
        public EventShopItemCategoryTypes Category { get; set; }
        public List<string> AccessRoles { get; set; } = new();
        public List<string> AccessSites { get; set; } = new();
        public Dictionary<string, object> Configuration { get; set; } = new();
        public long Point { get; set; }

        public string Language { get; set; } = "";
        public string Title { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; } = "";
        public List<string> Images { get; set; } = new();

        public (bool, EventShopItem?) ToEntity()
        {
            var item = new EventShopItem
            {
                Type = (short)Type,
                Category = (short)Category,
                AccessRoles = JsonConvert.SerializeObject(AccessRoles),
                AccessSites = JsonConvert.SerializeObject(AccessSites),
                Configuration = JsonConvert.SerializeObject(Configuration),
                Point = Point * 10000,
                Status = (short)EventShopItemStatusTypes.Draft,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                EventShopItemLanguages = new List<EventShopItemLanguage>
                {
                    new()
                    {
                        Language = Language,
                        Name = Name,
                        Title = Title,
                        Description = Description,
                        Images = JsonConvert.SerializeObject(Images),
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    }
                }
            };
            if (Type == EventShopItemTypes.Product) return (true, item);
            var configJson = JsonConvert.SerializeObject(Configuration);
            if (Type is EventShopItemTypes.ClientReward or EventShopItemTypes.AgentReward
                    or EventShopItemTypes.SalesReward && !RewardConfiguration.TryParse(configJson, out _))
                return (false, null);
            return (true, item);
        }

        public (bool, string) Validate() => Type switch
        {
            EventShopItemTypes.ClientReward => ValidateRewardConfiguration(),
            EventShopItemTypes.AgentReward => ValidateRewardConfiguration(),
            EventShopItemTypes.SalesReward => ValidateRewardConfiguration(),
            EventShopItemTypes.Product => ValidateProductConfiguration(),
            _ => (false, "Invalid type.")
        };

        private (bool, string) ValidateRewardConfiguration()
        {
            // Ensure configuration has effectiveFrom and effectiveTo
            var configJson = JsonConvert.SerializeObject(Configuration);
            return !RewardConfiguration.TryParse(configJson, out _)
                ? (false, "Configuration must have rewardType, effectiveFrom and effectiveTo.")
                : (true, "");
        }

        private (bool, string) ValidateProductConfiguration()
        {
            return (true, "");
        }
    }

    public class UpdateSpec
    {
        public EventShopItemTypes Type { get; set; }
        public EventShopItemCategoryTypes Category { get; set; }
        public List<string> AccessRoles { get; set; } = new();
        public List<string> AccessSites { get; set; } = new();
        public Dictionary<string, object> Configuration { get; set; } = new();
        public long Point { get; set; }
    }

    public class UpdateLanguageSpec
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string Title { get; set; } = "";
        public List<string> Images { get; set; } = new();
        public string? Description { get; set; }
    }
}