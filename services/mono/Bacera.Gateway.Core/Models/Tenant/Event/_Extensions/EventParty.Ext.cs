using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class EventParty
{
    public static EventParty Build(long partyId, EventPartyStatusTypes status = EventPartyStatusTypes.Applied)
        => new()
        {
            PartyId = partyId,
            OperatorPartyId = 10001,
            Status = (short)status,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            EventShopPoint = new EventShopPoint
            {
                Point = 0,
                TotalPoint = 0,
                FrozenPoint = 0,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            }
        };

    public static EventParty Build(long eventId, long partyId)
        => new()
        {
            EventId = eventId,
            PartyId = partyId,
            OperatorPartyId = 10001,
            Status = (short)EventPartyStatusTypes.Applied,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            EventShopPoint = new EventShopPoint
            {
                Point = 0,
                TotalPoint = 0,
                FrozenPoint = 0,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            }
        };

    public class TenantBaseModel
    {
        public string EventKey { get; set; } = "";
        public string Email { get; set; } = "";
        public string NativeName { get; set; } = "";
        public EventPartyStatusTypes Status { get; set; }
        public long Point { get; set; }
        public long TotalPoint { get; set; }
        public long FrozenPoint { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class TenantEventPartyPageModel : TenantBaseModel
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long PartyId { get; set; }
        public long OperatorPartyId { get; set; }
        public string LastOperatorName { get; set; } = "";
    }

    public class TenantEventPartyDetailModel : TenantBaseModel
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long PartyId { get; set; }
        public long OperatorPartyId { get; set; }
        public string LastOperatorName { get; set; } = "";
    }


    public class ClientBaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public string EventKey { get; set; } = "";
        public EventPartyStatusTypes Status { get; set; }
        public long Point { get; set; }
        public long TotalPoint { get; set; }
        public long FrozenPoint { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public sealed class ClientEventPartyDetailModel : ClientBaseModel
    {
        public ActiveReward? ActiveReward { get; set; }
        public List<ActiveReward> ActiveRewards { get; set; } = new();
    }

    public sealed class ActiveReward
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long ActiveRewardId { get; set; }

        public string ActiveRewardHashId => EventShopReward.HashEncode(ActiveRewardId);

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long ShopItemId { get; set; }

        public string ShopItemHashId => EventShopItem.HashEncode(ShopItemId);

        public string ShopItemName { get; set; } = "";

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ShopItemConfigurationJson { get; set; } = "";

        public long ValidPeriodInDays =>
            !EventShopItem.RewardConfiguration.TryParse(ShopItemConfigurationJson, out var configuration)
                ? 0
                : configuration.ValidPeriodInDays;

        public EventShopItem.RewardConfiguration Configuration =>
            EventShopItem.RewardConfiguration.TryParse(ShopItemConfigurationJson, out var configuration)
                ? configuration
                : new EventShopItem.RewardConfiguration();

        public DateTime EffectiveFrom => EffectiveTo.AddDays(-ValidPeriodInDays);
        public DateTime EffectiveTo { get; set; }
    }
}

public static class EventPartyExtensions
{
    public static IQueryable<EventParty.TenantEventPartyPageModel> ToTenantEventPartyPageModel(
        this IQueryable<EventParty> eventParties)
        => eventParties.Select(x => new EventParty.TenantEventPartyPageModel
        {
            Id = x.Id,
            EventId = x.EventId,
            PartyId = x.PartyId,
            EventKey = x.Event.Key,
            Email = x.Party.Email,
            NativeName = x.Party.NativeName,
            Status = (EventPartyStatusTypes)x.Status,
            OperatorPartyId = x.OperatorPartyId,
            LastOperatorName = x.OperatorParty.NativeName,
            Point = x.EventShopPoint.Point,
            TotalPoint = x.EventShopPoint.TotalPoint,
            FrozenPoint = x.EventShopPoint.FrozenPoint,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<EventParty.TenantEventPartyDetailModel> ToTenantDetailModel(
        this IQueryable<EventParty> eventParties)
        => eventParties.Select(x => new EventParty.TenantEventPartyDetailModel
        {
            Id = x.Id,
            EventId = x.EventId,
            PartyId = x.PartyId,
            EventKey = x.Event.Key,
            Email = x.Party.Email,
            NativeName = x.Party.NativeName,
            Status = (EventPartyStatusTypes)x.Status,
            OperatorPartyId = x.OperatorPartyId,
            LastOperatorName = x.OperatorParty.NativeName,
            Point = x.EventShopPoint.Point,
            TotalPoint = x.EventShopPoint.TotalPoint,
            FrozenPoint = x.EventShopPoint.FrozenPoint,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<EventParty.ClientEventPartyDetailModel> ToClientDetailModel(
        this IQueryable<EventParty> eventParties, string lang = "en-us")
        => eventParties
            .Select(x => new
            {
                x.Event,
                EventParty = x,
                x.EventShopPoint,
                ActiveRewards = x.EventShopRewards.Where(y => y.Status == (short)EventShopRewardStatusTypes.Active)
            })
            .Select(x => new EventParty.ClientEventPartyDetailModel
            {
                Id = x.EventParty.Id,
                EventKey = x.Event.Key,
                Status = (EventPartyStatusTypes)x.EventParty.Status,
                Point = x.EventShopPoint.Point,
                TotalPoint = x.EventShopPoint.TotalPoint,
                FrozenPoint = x.EventShopPoint.FrozenPoint,
                CreatedOn = x.EventParty.CreatedOn,
                UpdatedOn = x.EventParty.UpdatedOn,
                ActiveRewards = x.ActiveRewards.Select(y => new EventParty.ActiveReward
                {
                    ActiveRewardId = y.Id,
                    ShopItemId = y.EventPartyId,
                    ShopItemName = y.EventShopItem.EventShopItemLanguages.Any(z => z.Language == lang)
                        ? y.EventShopItem.EventShopItemLanguages.First(z => z.Language == lang).Name
                        : y.EventShopItem.EventShopItemLanguages.First(z => z.Language == "en-us").Name,
                    ShopItemConfigurationJson = y.EventShopItem.Configuration,
                    EffectiveTo = y.EffectiveTo
                }).ToList()
            });
}