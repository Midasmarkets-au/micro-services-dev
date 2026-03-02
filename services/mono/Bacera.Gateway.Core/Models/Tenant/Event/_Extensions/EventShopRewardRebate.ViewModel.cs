using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = EventShopRewardRebate;

public partial class EventShopRewardRebate
{
    public static readonly Hashids Hashids = new(HashIdSaltTypes.EventShopRewardRebate, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.EventShopRewardRebate]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public class BaseModel
    {
        public long Ticket { get; set; }
        public long Amount { get; set; }
        public double Volume { get; set; }
        public string Symbol { get; set; } = "";
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public string ServiceType { get; set; } = "";
        public EventShopRewardRebateStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class TenantBaseModel : BaseModel
    {
        public long Id { get; set; }
        public long EventShopRewardId { get; set; }
    }

    public sealed class TenantPageModel : TenantBaseModel
    {
        public Party.TenantBasicUserPageModel User { get; set; } = new();
    }

    public sealed class TenantDetailModel : TenantBaseModel
    {
        public Party.TenantBasicUserPageModel User { get; set; } = new();
    }

    public class ClientBaseModel : BaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long EventShopRewardId { get; set; }

        public string HashId => HashEncode(Id);
        public string EventShopRewardHashId => HashEncode(EventShopRewardId);
    }

    public sealed class ClientPageModel : ClientBaseModel
    {
    }

    public sealed class ClientDetailModel : ClientBaseModel
    {
    }
}

public static class EventShopRewardRebateViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query)
        => query.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            EventShopRewardId = x.EventShopRewardId,
            Ticket = x.Ticket,
            Amount = x.Amount,
            Status = (EventShopRewardRebateStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            User = new Party.TenantBasicUserPageModel
            {
                PartyId = x.EventShopReward.EventParty.PartyId,
                Email = x.EventShopReward.EventParty.Party.Email,
                Avatar = x.EventShopReward.EventParty.Party.Avatar,
                LastName = x.EventShopReward.EventParty.Party.LastName,
                FirstName = x.EventShopReward.EventParty.Party.FirstName,
                NativeName = x.EventShopReward.EventParty.Party.NativeName,
            }
        });

    public static IQueryable<M.TenantDetailModel> ToTenantDetailModel(this IQueryable<M> query)
        => query.Select(x => new M.TenantDetailModel
        {
            Id = x.Id,
            EventShopRewardId = x.EventShopRewardId,
            Ticket = x.Ticket,
            Amount = x.Amount,
            Status = (EventShopRewardRebateStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            User = new Party.TenantBasicUserPageModel
            {
                PartyId = x.EventShopReward.EventParty.PartyId,
                Email = x.EventShopReward.EventParty.Party.Email,
                Avatar = x.EventShopReward.EventParty.Party.Avatar,
                LastName = x.EventShopReward.EventParty.Party.LastName,
                FirstName = x.EventShopReward.EventParty.Party.FirstName,
                NativeName = x.EventShopReward.EventParty.Party.NativeName,
            }
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            EventShopRewardId = x.EventShopRewardId,
            Ticket = x.Ticket,
            Amount = x.Amount,
            Status = (EventShopRewardRebateStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
        });

    public static IQueryable<M.ClientDetailModel> ToClientDetailModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientDetailModel
        {
            Id = x.Id,
            EventShopRewardId = x.EventShopRewardId,
            Ticket = x.Ticket,
            Amount = x.Amount,
            Status = (EventShopRewardRebateStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
        });
}