namespace Bacera.Gateway;

partial class RebateSchemaBundle : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateSchemaBundle>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public RebateRuleTemplateBundleTypes? Type { get; set; }
        public long? CreatedBy { get; set; }
        public string? Keyword { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateSchemaBundle> pool)
        {
            pool.Add(x => x.Type == (int)Type!, Type.HasValue);
            pool.Add(x => x.CreatedBy == CreatedBy, CreatedBy.IsTangible());
            pool.Add(x => x.Name.Contains(Keyword!), !string.IsNullOrEmpty(Keyword));
        }
    }
}