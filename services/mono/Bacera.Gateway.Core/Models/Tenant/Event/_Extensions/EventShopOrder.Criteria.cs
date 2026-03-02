using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = EventShopOrder;

public partial class EventShopOrder : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public EventShopOrderStatusTypes? Status { get; set; }
        public List<EventShopOrderStatusTypes>? Statuses { get; set; }
        public long? PartyId { get; set; }
        public long? EventId { get; set; }
        public string? Email { get; set; }
        public string? EventKey { get; set; }
        public string? EventShopItemName { get; set; }
        public string? Name { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((EventShopOrderStatusTypes)x.Status), Statuses != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);

            pool.Add(x => x.EventParty.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.EventParty.Party.NativeName == Name, Name != null);
            pool.Add(x => x.EventParty.EventId == EventId, EventId != null);
            pool.Add(x => x.EventParty.Party.Email == Email, Email != null);
            pool.Add(x => x.EventShopItem.Event.Key == EventKey, EventKey != null);
            pool.Add(x => x.EventShopItem.EventShopItemLanguages.Any(l => l.Name == EventShopItemName),
                EventShopItemName != null);
        }
    }
}