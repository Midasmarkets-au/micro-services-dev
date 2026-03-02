namespace Bacera.Gateway;

using M = AccountPointTransaction;

partial class AccountPointTransaction : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? AccountUid { get; set; }
        public long? AccountId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To.HasValue);
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From.HasValue);
            pool.Add(x => x.AccountId.Equals(AccountId), AccountId.HasValue);
            pool.Add(x => x.Account.Uid.Equals(AccountUid), AccountUid.HasValue);
        }
    }
}