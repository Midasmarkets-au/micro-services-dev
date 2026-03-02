namespace Bacera.Gateway;

using M = MessageRecord;

partial class MessageRecord : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }


        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string? Receiver { get; set; }
        public long? ReceiverPartyId { get; set; }
        public string? Event { get; set; }
        public List<string>? Events { get; set; }
        public long? EventId { get; set; }
        public short? Status { get; set; }
        public string? Method { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());

            pool.Add(x => x.Receiver == Receiver, Receiver != null);
            pool.Add(x => x.ReceiverPartyId == ReceiverPartyId, ReceiverPartyId != null);
            pool.Add(x => x.Event == Event, Event != null);
            pool.Add(x => Events!.Contains(x.Event), Events != null);
            pool.Add(x => x.EventId == EventId, EventId != null);
            pool.Add(x => x.Status == Status, Status != null);
            pool.Add(x => x.Method == Method, Method != null);
        }
    }
}