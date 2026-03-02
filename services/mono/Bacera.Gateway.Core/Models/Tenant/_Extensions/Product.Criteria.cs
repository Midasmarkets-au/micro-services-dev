using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Product;

partial class Product : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
            SortFlag = false;
        }

        public ProductTypes? Type { get; set; }

        public ProductStatusType? Status { get; set; }

        public long? Point { get; set; }

        public long? TotalRemain { get; set; }
        public long? TotalLargerThan { get; set; }
        public long? TotalSmallerThan { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => x.Point == Point, Point.IsTangible());
            pool.Add(x => x.Total == TotalRemain, TotalRemain.IsTangible());
            pool.Add(x => x.Total > TotalLargerThan, TotalLargerThan.IsTangible());
            pool.Add(x => x.Total < TotalSmallerThan, TotalSmallerThan.IsTangible());
        }
    }
}