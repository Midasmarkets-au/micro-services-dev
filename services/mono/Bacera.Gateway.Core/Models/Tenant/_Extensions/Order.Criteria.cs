using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Order;

public partial class Order : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }

        public long? ProductId { get; set; }

        public long? Number { get; set; }

        public long? Amount { get; set; }

        public OrderStatusTypes? Status { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.ProductId == ProductId, ProductId.IsTangible());
            pool.Add(x => x.Number == Number, Number.IsTangible());
            pool.Add(x => x.Amount == Amount, Amount.IsTangible());
            pool.Add(x => x.Status == (short)Status!, Status.IsTangible());
        }
    }
}