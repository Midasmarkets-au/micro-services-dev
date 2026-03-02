namespace Bacera.Gateway;

using M = Tag;

partial class Tag : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public string? Keywords { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Name.Contains(Keywords!), !string.IsNullOrEmpty(Keywords));
        }
    }
}