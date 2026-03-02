namespace Bacera.Gateway;

using M = Payment;

partial class Payment : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public PaymentPlatformTypes? Platform { get; set; }
        public long? PartyId { get; set; }
        public LedgerSideTypes? LedgerSide { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Number { get; set; }
        public string? Keyword { get; set; }
        public long? MaxAmount { get; set; }
        public long? MixAmount { get; set; }
        public PaymentStatusTypes? Status { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.UpdatedOn <= To!.Value.ToUniversalTime(), To.HasValue && To.IsTangible());
            pool.Add(x => x.UpdatedOn >= From!.Value.ToUniversalTime(), From.HasValue && From.IsTangible());
            pool.Add(x => x.Amount >= MixAmount, MixAmount.HasValue && MixAmount.IsTangible());
            pool.Add(x => x.Amount <= MaxAmount!, MaxAmount.HasValue && MaxAmount.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue && CurrencyId.IsTangible());
            pool.Add(x => x.LedgerSide == (short)LedgerSide!, LedgerSide.HasValue && LedgerSide.IsTangible());
            pool.Add(x => x.Status == (short)Status!, Status.HasValue && Status.IsTangible());
            pool.Add(x => x.PaymentMethod.Platform == (int)Platform!, Platform.HasValue && Platform.IsTangible());

            pool.Add(x => x.Number == Number, Number is { Length: > 5 });
            pool.Add(x => x.Number.Contains(Keyword!), Keyword is { Length: > 3 });
        }
    }
}