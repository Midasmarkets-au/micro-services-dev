namespace Bacera.Gateway;

partial class RebateBaseSchema : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateBaseSchema>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? CreatedBy { get; set; }
        public string? Keyword { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateBaseSchema> pool)
        {
            pool.Add(x => x.CreatedBy == CreatedBy, CreatedBy.IsTangible());
            pool.Add(x => x.Name.Contains(Keyword!), !string.IsNullOrEmpty(Keyword));
        }
    }
}