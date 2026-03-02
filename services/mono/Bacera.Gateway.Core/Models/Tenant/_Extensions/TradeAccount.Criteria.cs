namespace Bacera.Gateway;

using M = TradeAccount;

partial class TradeAccount : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public long? Uid { get; set; }
        public long? PartyId { get; set; }
        public int? ServiceId { get; set; }
        public long? AccountNumber { get; set; }

        public long? SalesUid { get; set; }
        public long? AgentUid { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public FundTypes? FundType { get; set; }
        public bool? IncludeClosed { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.IdNavigation.IsClosed == 0 && x.IdNavigation.Status == 0, IncludeClosed is not true);
            pool.Add(x => x.IdNavigation.Uid.Equals(Uid), Uid.HasValue);
            pool.Add(x => x.ServiceId == ServiceId, ServiceId.IsTangible());
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.IsTangible());
            pool.Add(x => x.IdNavigation.PartyId.Equals(PartyId), PartyId.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue);
            pool.Add(x => x.IdNavigation.FundType == (int)FundType!, FundType.HasValue);

            pool.Add(x => x.IdNavigation.AgentAccount != null && x.IdNavigation.AgentAccount.Uid == AgentUid,
                AgentUid.IsTangible());

            pool.Add(x => x.IdNavigation.SalesAccount != null && x.IdNavigation.SalesAccount.Uid == SalesUid,
                SalesUid.IsTangible());
        }
    }
}

public static class TradeAccountCriteriaExtensions
{
    public static TradeAccount.Criteria IncludeClosed(this TradeAccount.Criteria criteria)
    {
        criteria.IncludeClosed = true;
        return criteria;
    }
}