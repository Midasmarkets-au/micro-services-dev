namespace Bacera.Gateway;

using M = Transaction;

partial class Transaction : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Withdrawal.Id);
        }

        public long? PartyId { get; set; }
        public long? TotalAmount { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public StateTypes? StateId { get; set; }
        public int? SourceAccountType { get; set; }
        public long? SourceAccountId { get; set; }
        public int? TargetAccountType { get; set; }
        public int? TargetAccountId { get; set; }
        public TransactionAccountTypes? TransferType { get; set; }
        public long? WalletId { get; set; }
        public long? AccountId { get; set; }
        public long? ParentAccountUid { get; set; }
        public string? Email { get; set; }
        public string? Target { get; set; }
        public long? AccountNumber { get; set; }
        public List<int>? StateIds { get; set; }
        public AccountRoleTypes? Role { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());

            pool.Add(x => x.SourceAccountType == SourceAccountType, SourceAccountType.IsTangible());
            pool.Add(x => x.TargetAccountType == TargetAccountType, TargetAccountType.IsTangible());

            pool.Add(x => x.SourceAccountId == SourceAccountId, SourceAccountId.IsTangible());
            pool.Add(x => x.TargetAccountId == TargetAccountId, TargetAccountId.IsTangible());

            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId.IsTangible());
            pool.Add(x => StateIds!.Contains(x.IdNavigation.StateId), StateIds != null && StateIds.Count != 0);
            pool.Add(x => x.IdNavigation.StatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());
            pool.Add(x => x.Party.Email == Email!, Email != null);
            pool.Add(x => x.Party.Accounts.Any(a => a.Status == 0 && a.Role == (int)Role!), Role != null);

            // for wallet
            pool.Add(x =>
                    (x.SourceAccountType == (int)TransactionAccountTypes.Wallet &&
                     x.SourceAccountId == WalletId)
                    || (x.TargetAccountType == (int)TransactionAccountTypes.Wallet &&
                        x.TargetAccountId == WalletId)
                , WalletId.IsTangible());

            // for Trade Account
            pool.Add(x =>
                    (x.SourceAccountType == (int)TransactionAccountTypes.Account &&
                     x.SourceAccountId == AccountId)
                    || (
                        x.TargetAccountType == (int)TransactionAccountTypes.Account &&
                        x.TargetAccountId == AccountId),
                AccountId != null);

            pool.Add(x => x.SourceAccountType == (int)TransferType! || x.TargetAccountType == (int)TransferType!
                , TransferType.IsTangible());
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long? PartyId { get; set; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long? AccountId { get; set; }


        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long? WalletId { get; set; }

        public CurrencyTypes? CurrencyId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public StateTypes? StateId { get; set; }

        public List<int>? StateIds { get; set; }

        public PaymentStatusTypes? PaymentStatus { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId.IsTangible());
            pool.Add(x => StateIds!.Contains(x.IdNavigation.StateId), StateIds != null && StateIds.Count != 0);
            pool.Add(x => x.IdNavigation.StatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());

            pool.Add(x => x.SourceAccountType == (short)TransactionAccountTypes.Account && x.SourceAccountId == AccountId ||
                          x.TargetAccountType == (short)TransactionAccountTypes.Account && x.TargetAccountId == AccountId, AccountId != null);

            pool.Add(x => x.SourceAccountType == (short)TransactionAccountTypes.Wallet && x.SourceAccountId == WalletId ||
                          x.TargetAccountType == (short)TransactionAccountTypes.Wallet && x.TargetAccountId == WalletId, WalletId != null);
        }
    }
}