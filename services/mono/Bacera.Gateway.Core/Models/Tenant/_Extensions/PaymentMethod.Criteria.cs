namespace Bacera.Gateway;

using M = PaymentMethod;

partial class PaymentMethod : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortFlag = false;
        }

        public List<CurrencyTypes>? AvailableCurrencies { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
        }
    }
}