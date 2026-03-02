using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = EventShopReward;

public partial class EventShopReward
{
    public static readonly Hashids Hashids = new(HashIdSaltTypes.EventShopReward, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.EventShopReward]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public class BaseModel
    {
        public string EventShopItemName { get; set; } = string.Empty;
        public long TotalPoint { get; set; }
        public EventShopRewardStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime EffectiveTo { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ConfigurationJson { get; set; } = string.Empty;

        public EventShopItem.RewardConfiguration Configuration =>
            Utils.JsonDeserializeObjectWithDefault<EventShopItem.RewardConfiguration>(ConfigurationJson);
    }

    public class TenantBaseModel : BaseModel
    {
        public long Id { get; set; }
        public long PartyId { get; set; }
        public long EventPartyId { get; set; }
        public string EventPartyNativeName { get; set; } = string.Empty;
        public string EventPartyEmail { get; set; } = string.Empty;
        public long EventShopItemId { get; set; }
        public long OperatorPartyId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
    }

    public class TenantPageModel : TenantBaseModel
    {
    }

    public class TenantDetailModel : TenantBaseModel
    {
    }

    public class ClientBaseModel : BaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public string HashId => HashEncode(Id);

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long ShopItemId { get; set; }

        public string ShopItemHashId => EventShopItem.HashEncode(ShopItemId);
    }

    public sealed class ClientPageModel : ClientBaseModel
    {
    }

    public sealed class ClientDetailModel : ClientBaseModel
    {
    }
}

public static class EventShopRewardViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            PartyId = x.EventParty.PartyId,
            EventPartyId = x.EventPartyId,
            EventPartyNativeName = x.EventParty.Party.NativeName,
            EventPartyEmail = x.EventParty.Party.Email,
            EventShopItemId = x.EventShopItemId,
            ConfigurationJson = x.EventShopItem.Configuration,
            EventShopItemName =
                x.EventShopItem.EventShopItemLanguages
                    .Any(y => y.Language == lang)
                    ? x.EventShopItem.EventShopItemLanguages
                        .First(y => y.Language == lang).Name
                    : "",
            TotalPoint = x.TotalPoint,
            Status = (EventShopRewardStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            EffectiveTo = x.Status == (short)EventShopRewardStatusTypes.Approved ? x.EffectiveTo : DateTime.MinValue,
            OperatorPartyId = x.OperatorPartyId,
            OperatorName = x.OperatorParty.NativeName
        });

    public static IQueryable<M.TenantDetailModel> ToTenantDetailModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.TenantDetailModel
        {
            Id = x.Id,
            PartyId = x.EventParty.PartyId,
            EventPartyId = x.EventPartyId,
            EventPartyNativeName = x.EventParty.Party.NativeName,
            EventPartyEmail = x.EventParty.Party.Email,
            EventShopItemId = x.EventShopItemId,
            ConfigurationJson = x.EventShopItem.Configuration,
            EventShopItemName =
                x.EventShopItem.EventShopItemLanguages
                    .Any(y => y.Language == lang)
                    ? x.EventShopItem.EventShopItemLanguages
                        .First(y => y.Language == lang).Name
                    : "",
            TotalPoint = x.TotalPoint,
            Status = (EventShopRewardStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            EffectiveTo = x.Status == (short)EventShopRewardStatusTypes.Approved ? x.EffectiveTo : DateTime.MinValue,
            OperatorPartyId = x.OperatorPartyId,
            OperatorName = x.OperatorParty.NativeName,
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            ShopItemId = x.EventShopItemId,
            EventShopItemName =
                x.EventShopItem.EventShopItemLanguages
                    .Any(y => y.Language == lang)
                    ? x.EventShopItem.EventShopItemLanguages
                        .First(y => y.Language == lang).Name
                    : "",
            ConfigurationJson = x.EventShopItem.Configuration,
            TotalPoint = x.TotalPoint,
            Status = (EventShopRewardStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            EffectiveTo = x.Status == (short)EventShopRewardStatusTypes.Approved ? x.EffectiveTo : DateTime.MinValue,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<M.ClientDetailModel> ToClientDetailModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.ClientDetailModel
        {
            Id = x.Id,
            ShopItemId = x.EventShopItemId,
            EventShopItemName =
                x.EventShopItem.EventShopItemLanguages
                    .Any(y => y.Language == lang)
                    ? x.EventShopItem.EventShopItemLanguages
                        .First(y => y.Language == lang).Name
                    : "",
            ConfigurationJson = x.EventShopItem.Configuration,
            TotalPoint = x.TotalPoint,
            Status = (EventShopRewardStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            EffectiveTo = x.Status == (short)EventShopRewardStatusTypes.Approved ? x.EffectiveTo : DateTime.MinValue,
            UpdatedOn = x.UpdatedOn,
        });
}