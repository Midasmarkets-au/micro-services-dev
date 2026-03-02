using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = EventShopPointTransaction;

public partial class EventShopPointTransaction : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public EventShopPointTransactionStatusTypes? Status { get; set; }
        public EventShopPointTransactionSourceTypes? SourceType { get; set; }
        public long? PartyId { get; set; }
        public long? AccountNumber { get; set; }
        public long? ServiceId { get; set; }
        public long? EventId { get; set; }
        public string? EventKey { get; set; }
        public long? Ticket { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        // account number, account type, service type

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => x.SourceType == (short)SourceType!, SourceType != null);
            pool.Add(x => x.EventParty.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.EventParty.EventId == EventId, EventId != null);
            pool.Add(x => x.EventParty.Event.Key == EventKey, EventKey != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);

            pool.Add(x => x.SourceContent.Contains(AccountNumber!.Value.ToString()), AccountNumber != null);
            pool.Add(x => x.SourceContent.Contains($"ServiceId\":{ServiceId!.Value}"), ServiceId != null);
            pool.Add(x => x.SourceContent.Contains($"Ticket\":{Ticket!.Value}"), Ticket != null);
        }
    }
}