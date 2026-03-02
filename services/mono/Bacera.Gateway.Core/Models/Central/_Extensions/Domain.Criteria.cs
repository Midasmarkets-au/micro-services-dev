namespace Bacera.Gateway;

using M = Domain;

partial class Domain : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public string? Keyword { get; set; }
        public long? TenantId { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.DomainName.ToUpper().Contains(Keyword!.ToUpper()), !string.IsNullOrEmpty(Keyword));
            pool.Add(x => x.TenantId == TenantId, TenantId is > 0);
        }
    }
}