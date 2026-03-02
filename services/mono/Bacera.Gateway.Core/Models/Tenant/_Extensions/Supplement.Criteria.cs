namespace Bacera.Gateway;

using M = Supplement;

partial class Supplement
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public SupplementTypes? Type { get; set; }
        public long? RowId { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type != null && Type.IsTangible());
            pool.Add(x => x.RowId == RowId, RowId.HasValue);
        }
    }
}