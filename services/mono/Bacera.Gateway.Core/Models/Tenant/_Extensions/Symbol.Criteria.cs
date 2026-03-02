using M = Bacera.Gateway.Symbol;

namespace Bacera.Gateway;

partial class Symbol : IEntity<int>
{
    public sealed class Criteria : EntityCriteria<M, int>
    {
        public int? CategoryId { get; set; }
        public string? Code { get; set; }
        public string? Category { get; set; }
        

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CategoryId == CategoryId, CategoryId.IsTangible());
            pool.Add(x => x.Code.ToUpper() == Code!.ToUpper(), Code != null && !string.IsNullOrEmpty(Code) && Code.IsTangible());
            pool.Add(x => x.Category.Contains(Category!),
                Category != null && !string.IsNullOrEmpty(Category) && Category.IsTangible() && Category.Length >= 2);
        }
    }
}