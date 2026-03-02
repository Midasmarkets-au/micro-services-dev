using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = EventShopRewardRebate;

public partial class EventShopRewardRebate : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public EventShopRewardRebateStatusTypes? Status { get; set; }
        public List<EventShopRewardRebateStatusTypes>? Statuses { get; set; }
        public long? PartyId { get; set; }
        public long? EventId { get; set; }
        public string? EventKey { get; set; }
        public string? Email { get; set; }
        public string? NativeName { get; set; }
        public long? Ticket { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Ticket == Ticket, Ticket != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((EventShopRewardRebateStatusTypes)x.Status), Statuses != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);

            pool.Add(x => x.EventShopReward.EventParty.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.EventShopReward.EventParty.EventId == EventId, EventId != null);
            pool.Add(x => x.EventShopReward.EventParty.Event.Key == EventKey, EventKey != null);
            pool.Add(x => x.EventShopReward.EventParty.Party.Email == Email, Email != null);
            pool.Add(x => x.EventShopReward.EventParty.Party.NativeName == NativeName, NativeName != null);
        }
    }
}