namespace Bacera.Gateway;

using M = WalletDailySnapshot;

partial class WalletDailySnapshot : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public FundTypes? FundType { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public string? Email { get; set; }
        public bool? HasBalance { get; set; }
        public DateTime? SnapshotDate { get; set; }
        public bool? UseMT5Time { get; set; }  // true = use real-time balance (23:59:59), false/null = use snapshot table (22:00)

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Wallet.FundType == (int)FundType!, FundType.IsTangible());
            pool.Add(x => x.Wallet.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => (bool)HasBalance! ? x.Balance > 0 || x.Balance < 0 : x.Balance == 0, HasBalance != null);
            pool.Add(x => x.Wallet.Party.Email == Email!, Email != null);
            pool.Add(x => x.Wallet.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.SnapshotDate > SnapshotDate!.Value.ToUniversalTime().Date
                          && x.SnapshotDate < SnapshotDate!.Value.ToUniversalTime().Date.AddDays(1), SnapshotDate != null);
        }
    }
}