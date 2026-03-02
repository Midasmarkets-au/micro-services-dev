namespace Bacera.Gateway;

partial class RebateBrokerRule : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateBrokerRule>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? BrokerAccountId { get; set; }
        public long? BrokerAccountUid { get; set; }
        public long? SalesUid { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateBrokerRule> pool)
        {
            pool.Add(x => x.BrokerAccountId == BrokerAccountId, BrokerAccountId.IsTangible());
            pool.Add(x => x.BrokerAccount.Uid == BrokerAccountUid, BrokerAccountUid.IsTangible());
        }
    }
}