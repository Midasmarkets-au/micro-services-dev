namespace Bacera.Gateway;

using M = ExchangeRate;

partial class ExchangeRate : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public CurrencyTypes? FromCurrencyId { get; set; }
        public CurrencyTypes? ToCurrencyId { get; set; }

        public List<int>? FromCurrencyIds { get; set; }
        public List<int>? ToCurrencyIds { get; set; }
        public List<string>? Names { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.ToCurrencyId == (int)ToCurrencyId!, ToCurrencyId.HasValue);
            pool.Add(x => x.FromCurrencyId == (int)FromCurrencyId!, FromCurrencyId.HasValue);
            pool.Add(x => x.ToCurrencyId == (int)ToCurrencyId!, ToCurrencyId.HasValue);
            pool.Add(x => ToCurrencyIds!.Contains(x.ToCurrencyId),
                ToCurrencyIds != null && ToCurrencyIds.Any());
            pool.Add(x => FromCurrencyIds!.Contains(x.FromCurrencyId),
                FromCurrencyIds != null && FromCurrencyIds.Any());
            pool.Add(x => Names!.Contains(x.Name),
                Names != null && Names.Any());
        }
    }
}