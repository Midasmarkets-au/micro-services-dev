namespace Bacera.Gateway;

using M = Tenant;

partial class Tenant : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public string? Keyword { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Name.ToUpper().Contains(Keyword!.ToUpper()) || x.DatabaseName.Contains(Keyword),
                !string.IsNullOrEmpty(Keyword));
        }
    }
}