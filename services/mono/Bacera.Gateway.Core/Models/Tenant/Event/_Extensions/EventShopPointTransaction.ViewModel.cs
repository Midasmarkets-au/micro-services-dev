using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;
using M = Bacera.Gateway.EventShopPointTransaction;

namespace Bacera.Gateway;

public partial class EventShopPointTransaction
{
    public class MQSource
    {
        public EventShopPointTransactionSourceTypes SourceType { get; set; }
        public long RowId { get; set; }
        public long TenantId { get; set; }

        public static MQSource Build(EventShopPointTransactionSourceTypes sourceType, long rowId, long tenantId)
            => new() { SourceType = sourceType, RowId = rowId, TenantId = tenantId };

        public override string ToString() => JsonConvert.SerializeObject(this);

        public string ToRedisKey() => $"event_shop_mq_src_tid:{TenantId}_sourceType:{SourceType}_rowId{RowId}";

        public static bool TryParse(string json, out MQSource source)
        {
            source = new MQSource();
            try
            {
                source = JsonConvert.DeserializeObject<MQSource>(json)!;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class BaseModel
    {
        public long Point { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long SourceId { get; set; }

        public EventShopPointTransactionSourceTypes SourceType { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SourceContentJson { get; set; } = "{}";

        public EventShopPointTransactionStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public long TargetAccountUid { get; set; }

        public long Ticket { get; set; }

        public class TradeSource
        {
            public TradeSource(long accountNumber, long ticket, double volume, int serviceId)
            {
                AccountNumber = accountNumber;
                Ticket = ticket;
                Volume = volume;
                ServiceId = serviceId;
            }

            public long AccountNumber { get; }
            public long Ticket { get; }
            public double Volume { get; }
            public int ServiceId { get; }
        }

        public object Source
        {
            get
            {
                if (SourceType == EventShopPointTransactionSourceTypes.Trade)
                {
                    var trade = JsonConvert.DeserializeObject<TradeViewModel>(SourceContentJson);
                    if (trade == null) return new { };
                    return new TradeSource(trade.AccountNumber, trade.Ticket, trade.Volume, trade.ServiceId);
                }

                return new { };
            }
        }
    }

    public class TenantBaseModel : BaseModel
    {
        public long Id { get; set; }
        public long EventPartyId { get; set; }
    }

    public sealed class TenantPageModel : TenantBaseModel
    {
        public Party.TenantBasicUserPageModel User { get; set; } = new();
    }

    public class ClientPageModel : BaseModel
    {
    }

    public static M Build(long eventPartyId, EventShopPointTransactionSourceTypes sourceType,
        long point, string sourceContent = "{}")
        => new()
        {
            EventPartyId = eventPartyId,
            Point = point,
            SourceType = (short)sourceType,
            SourceContent = sourceContent,
            Status = (short)EventShopPointTransactionStatusTypes.Pending,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };
}

public static class EventShopPointTransactionViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q)
        => q.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            EventPartyId = x.EventPartyId,
            SourceId = x.SourceId,
            Point = x.Point,
            SourceType = (EventShopPointTransactionSourceTypes)x.SourceType,
            SourceContentJson = x.SourceContent,
            Status = (EventShopPointTransactionStatusTypes)x.Status,
            TargetAccountUid = x.Account != null ? x.Account.Uid : 0,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            User = new Party.TenantBasicUserPageModel
            {
                PartyId = x.EventParty.Party.Id,
                Email = x.EventParty.Party.Email,
                Avatar = x.EventParty.Party.Avatar,
                LastName = x.EventParty.Party.LastName,
                FirstName = x.EventParty.Party.FirstName,
                NativeName = x.EventParty.Party.NativeName,
            }
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(
        this IQueryable<M> q)
        => q.Select(x => new M.ClientPageModel
        {
            Point = x.Point,
            SourceId = x.SourceId,
            SourceType = (EventShopPointTransactionSourceTypes)x.SourceType,
            SourceContentJson = x.SourceContent,
            Status = (EventShopPointTransactionStatusTypes)x.Status,
            TargetAccountUid = x.Account != null ? x.Account.Uid : 0,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });
}