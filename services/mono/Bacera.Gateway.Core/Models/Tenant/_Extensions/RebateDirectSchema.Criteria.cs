namespace Bacera.Gateway;

partial class RebateDirectSchema : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateDirectSchema>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? CreatedBy { get; set; }
        public long? ConfirmedBy { get; set; }
        public bool? IsConfirmed { get; set; }
        public string? Keyword { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateDirectSchema> pool)
        {
            pool.Add(x => x.CreatedBy == CreatedBy, CreatedBy.IsTangible());
            pool.Add(x => x.CreatedBy > 1, CreatedBy.IsTangible());
            pool.Add(x => x.ConfirmedBy == ConfirmedBy, ConfirmedBy.IsTangible());
            pool.Add(x => x.ConfirmedOn != null, IsConfirmed is true);
            pool.Add(x => x.ConfirmedOn == null, IsConfirmed is false);
            pool.Add(x => x.Name.Contains(Keyword!), !string.IsNullOrEmpty(Keyword));
        }
    }
}