namespace Bacera.Gateway;

partial class RebateClientRule : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateClientRule>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? ClientAccountId { get; set; }
        public long? ClientAccountUid { get; set; }
        public long? ParentAgentId { get; set; }
        public long? ParentAgentUid { get; set; }
        public long? SalesUid { get; set; }
        public long? SalesId { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateClientRule> pool)
        {
            pool.Add(x => x.ClientAccountId == ClientAccountId, ClientAccountId.IsTangible());
            pool.Add(x => x.ClientAccount.Uid == ClientAccountUid, ClientAccountUid.IsTangible());

            pool.Add(x => x.ClientAccount.AgentAccount != null && x.ClientAccount.AgentAccount.Uid == ParentAgentUid,
                ParentAgentUid.IsTangible());

            pool.Add(x => x.ClientAccount.AgentAccountId == ParentAgentId,
                ParentAgentId.IsTangible());

            pool.Add(x => x.ClientAccount.SalesAccount != null && x.ClientAccount.SalesAccount.Uid == SalesUid,
                SalesUid.IsTangible());

            pool.Add(x => x.ClientAccount.SalesAccountId == SalesId,
                SalesId.IsTangible());
        }
    }
}