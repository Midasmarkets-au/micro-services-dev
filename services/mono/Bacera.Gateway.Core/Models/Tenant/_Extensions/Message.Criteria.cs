namespace Bacera.Gateway;

using M = Message;

partial class Message : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }

        public long? ReferenceId { get; set; }
        public MessageReferenceTypes? ReferenceType { get; set; }

        public long? SenderId { get; set; }
        public MessageSenderTypes? SenderType { get; set; }

        public MessageTypes? Type { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool? IsRead { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());

            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());

            pool.Add(x => x.ReferenceId == ReferenceId, ReferenceId.HasValue);
            pool.Add(x => x.ReferenceType == (int)ReferenceType!, ReferenceType.HasValue);

            pool.Add(x => x.SenderId == SenderId, SenderId.HasValue);
            pool.Add(x => x.SenderType == (int)SenderType!, SenderType.HasValue);

            pool.Add(x => x.ReadOn != null, IsRead is true);
            pool.Add(x => x.ReadOn == null, IsRead is false);
        }
    }
}