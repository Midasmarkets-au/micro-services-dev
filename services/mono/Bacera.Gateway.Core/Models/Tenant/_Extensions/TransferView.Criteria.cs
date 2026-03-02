using M = Bacera.Gateway.TransferView;

namespace Bacera.Gateway;

partial class TransferView : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(StatedOn);
        }

        public long? PartyId { get; set; }
        public MatterTypes? MatterType { get; set; }
        public FundTypes? FundType { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public StateTypes? StateId { get; set; }
        public List<int>? StateIds { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public LedgerSideTypes? LedgerSide { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId != null && CurrencyId.IsTangible());
            pool.Add(x => x.FundType == (int)FundType!, FundType != null && FundType.IsTangible());
            pool.Add(x => x.LedgerSide == (int)LedgerSide!, LedgerSide != null && LedgerSide.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.PostedOn <= To!.Value.ToUniversalTime(), To != null && To.IsTangible());
            pool.Add(x => x.PostedOn >= From!.Value.ToUniversalTime(), From != null && From.IsTangible());
            pool.Add(x => x.StateId == (int)StateId!, StateId != null && StateId.IsTangible());
            pool.Add(x => x.StateId != null && StateIds!.Contains((int)x.StateId), StateIds != null && StateIds.Any());
            pool.Add(x => x.Type == (int)MatterType!, MatterType != null && MatterType.IsTangible());
        }
    }
}