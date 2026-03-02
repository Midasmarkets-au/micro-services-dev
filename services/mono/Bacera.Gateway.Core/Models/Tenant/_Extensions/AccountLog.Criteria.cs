namespace Bacera.Gateway;

using M = AccountLog;

public partial class AccountLog : IEntity
{
    public sealed class TenantCriteria : EntityCriteria<M>
    {
        public TenantCriteria()
        {
            SortField = nameof(M.Id);
        }
        public string? Action { get; set; }
        public long? AccountId { get; set; }
        public long? AccountNumber { get; set; }
        public string? SearchText { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Action.Contains(Action!), !string.IsNullOrEmpty(Action));
            pool.Add(x => x.AccountId == AccountId, AccountId.HasValue);
            pool.Add(x => x.Account.AccountNumber == AccountNumber, AccountNumber.HasValue);
            pool.Add(x => x.Account.SearchText.Contains(SearchText!), !string.IsNullOrEmpty(SearchText));
        }
    }

    public sealed class ClientCriteria : EntityCriteria<M>
    {
        public string? Action { get; set; }
        public long? AccountNumber { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Action.Contains(Action!), !string.IsNullOrEmpty(Action));
            pool.Add(x => x.Account.AccountNumber == AccountNumber, AccountNumber.HasValue);
        }
    }
}