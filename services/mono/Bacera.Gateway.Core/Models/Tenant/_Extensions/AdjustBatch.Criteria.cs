using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = AdjustBatch;

public partial class AdjustBatch : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public AdjustTypes? Type { get; set; }
        public AdjustBatchStatusTypes? Status { get; set; }
        public List<AdjustBatchStatusTypes>? Statuses { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((AdjustBatchStatusTypes)x.Status), Statuses != null);
        }
    }
}