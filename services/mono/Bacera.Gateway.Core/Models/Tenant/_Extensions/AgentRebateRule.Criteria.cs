namespace Bacera.Gateway;

partial class RebateAgentRule : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateAgentRule>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? AgentId { get; set; }
        public long? AgentUid { get; set; }
        public List<long>? AgentUids { get; set; }
        public long? ParentAgentUid { get; set; }
        public long? SalesUid { get; set; }
        public long? RepUid { get; set; }
        public bool? ViewAllLevel { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateAgentRule> pool)
        {
            pool.Add(x => x.AgentAccountId == AgentId, AgentId.IsTangible());
            pool.Add(x => x.AgentAccount.Uid == AgentUid, AgentUid.IsTangible());

            pool.Add(x => AgentUids!.Contains(x.AgentAccount.Uid),
                AgentUids != null && AgentUids.Any() && AgentUids.IsTangible());

            pool.Add(x => x.AgentAccount.ReferPath.StartsWith("." + RepUid!.Value),
                RepUid.IsTangible());

            pool.Add(x => x.AgentAccount.SalesAccount != null && x.AgentAccount.SalesAccount.Uid == SalesUid,
                SalesUid.IsTangible() && ViewAllLevel is not true);
            
            pool.Add(x => x.AgentAccount.ReferPath.Contains(SalesUid!.Value.ToString()),
                SalesUid.IsTangible() && ViewAllLevel is true);

            pool.Add(x => x.AgentAccount.AgentAccount != null && x.AgentAccount.AgentAccount.Uid == ParentAgentUid,
                ParentAgentUid.IsTangible());
        }
    }
}