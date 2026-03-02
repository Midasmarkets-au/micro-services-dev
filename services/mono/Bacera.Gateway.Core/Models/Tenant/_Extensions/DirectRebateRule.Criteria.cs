namespace Bacera.Gateway;

partial class RebateDirectRule : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateDirectRule>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? SourceAccountId { get; set; }
        public long? TargetAccountId { get; set; }
        public long? SourceAccountUid { get; set; }
        public long? TargetAccountUid { get; set; }
        public bool? IsConfirmed { get; set; }

        protected override void OnCollect(ICriteriaPool<RebateDirectRule> pool)
        {
            pool.Add(x => x.SourceTradeAccount.IdNavigation.Uid == SourceAccountUid, SourceAccountUid.IsTangible());
            pool.Add(x => x.TargetAccount.Uid == TargetAccountUid, TargetAccountUid.IsTangible());
            pool.Add(x => x.SourceTradeAccountId == SourceAccountId, SourceAccountId.IsTangible());
            pool.Add(x => x.TargetAccountId == TargetAccountId, TargetAccountId.IsTangible());
            pool.Add(x => x.ConfirmedBy != null, IsConfirmed == true);
            pool.Add(x => x.ConfirmedBy == null, IsConfirmed == false);
        }
    }
}