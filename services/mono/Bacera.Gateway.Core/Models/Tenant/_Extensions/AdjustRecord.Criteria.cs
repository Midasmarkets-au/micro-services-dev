using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = AdjustRecord;

public partial class AdjustRecord : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? AccountNumber { get; set; }
        public long? AdjustBatchId { get; set; }
        public long? Ticket { get; set; }
        public AdjustTypes? Type { get; set; }
        public AdjustRecordStatusTypes? Status { get; set; }
        public List<AdjustRecordStatusTypes>? Statuses { get; set; }

        public long? AccountId { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber != null);
            pool.Add(x => x.AdjustBatchId == (AdjustBatchId != 0 ? AdjustBatchId : null), AdjustBatchId != null);
            pool.Add(x => x.AccountId == AccountId, AccountId != null);
            pool.Add(x => x.Type == (short)Type!, Type != null);
            pool.Add(x => x.Ticket == Ticket, Ticket != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((AdjustRecordStatusTypes)x.Status), Statuses != null);
        }
    }
}