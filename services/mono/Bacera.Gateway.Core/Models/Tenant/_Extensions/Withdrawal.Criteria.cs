namespace Bacera.Gateway;

partial class Withdrawal : IEntity
{
    public sealed class Criteria : EntityCriteria<Withdrawal>
    {
        public Criteria()
        {
            SortField = nameof(Withdrawal.Id);
        }

        public static Criteria BuildForPeriod(DateTime? from, DateTime? to) => new()
        {
            From = from,
            To = to,
            StateId = StateTypes.WithdrawalCompleted,
        };

        public long? PartyId { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }

        public FundTypes? FundType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? ApprovedFrom { get; set; }
        public DateTime? ApprovedTo { get; set; }
        public DateTime? To { get; set; }
        public StateTypes? StateId { get; set; }
        public CurrencyTypes? PaymentCurrencyId { get; set; }
        public PaymentStatusTypes? PaymentStateId { get; set; }
        public TransactionAccountTypes? SourceType { get; set; }
        public AccountRoleTypes? Role { get; set; }
        public string? Email { get; set; }
        public string? Target { get; set; }
        public long? TotalAmount { get; set; }

        public long? AccountUid { get; set; }
        public long? WalletId { get; set; }
        public long? AccountId { get; set; }
        public long? PaymentId { get; set; }
        public long? AccountNumber { get; set; }
        public long? ParentAccountUid { get; set; }

        public List<int>? StateIds { get; set; }


        protected override void OnCollect(ICriteriaPool<Withdrawal> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.FundType == (int)FundType!, FundType.HasValue);
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue);
            pool.Add(x => x.IdNavigation.StatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());

            pool.Add(x => x.ApprovedOn >= ApprovedFrom!.Value.ToUniversalTime(), ApprovedFrom.IsTangible());

            pool.Add(x => x.ApprovedOn <= ApprovedTo!.Value.ToUniversalTime(), ApprovedTo.IsTangible());
            
            
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId.IsTangible());
            pool.Add(x => x.PaymentId == PaymentId, PaymentId != null);
            pool.Add(x => StateIds!.Contains(x.IdNavigation.StateId), StateIds != null && StateIds.Count != 0);
            pool.Add(x => x.Payment.CurrencyId == (int)PaymentCurrencyId!, PaymentCurrencyId.IsTangible());
            pool.Add(x => x.Payment.Status == (short)PaymentStateId!, PaymentStateId.HasValue);
            pool.Add(x => x.Party.Email == Email!, Email != null);

            pool.Add(x => x.SourceAccount != null && x.SourceAccount.Role == (short)AccountRoleTypes.Client,
                Role is AccountRoleTypes.Client);

            pool.Add(x => x.SourceAccount == null,
                Role is AccountRoleTypes.Agent or AccountRoleTypes.Sales);

            pool.Add(
                x => SourceType == TransactionAccountTypes.Wallet
                    ? x.SourceAccount == null
                    : x.SourceAccount != null, SourceType.IsTangible());

            // pool.Add(x => x.SourceWalletId == WalletId!.Value,
            //     WalletId != null);

            pool.Add(
                x => (x.SourceAccount != null && x.SourceAccount.Role == (short)AccountRoleTypes.Agent)
                     || x.Party.Accounts.Any(y => y.ReferPath.Contains(ParentAccountUid!.Value.ToString())),
                Role is AccountRoleTypes.Agent && ParentAccountUid != null);

            pool.Add(x => x.SourceAccount != null
                          && x.SourceAccount.ReferPath.Contains(ParentAccountUid!.Value.ToString()),
                ParentAccountUid != null);

            pool.Add(x => x.SourceAccount != null
                          && x.SourceAccount.Uid == AccountUid
                          && (ParentAccountUid == null
                              || x.SourceAccount.ReferPath.Contains(ParentAccountUid.Value.ToString()))
                , AccountUid != null);

            pool.Add(x => x.SourceAccount != null
                          && x.SourceAccount.AccountNumber == AccountNumber
                          && (ParentAccountUid == null
                              || x.SourceAccount.ReferPath.Contains(ParentAccountUid.Value.ToString()))
                , AccountNumber != null);

            pool.Add(x => x.SourceAccountId != null
                          && x.SourceAccount != null
                          && x.SourceAccountId == AccountId
                          && (ParentAccountUid == null
                              || x.SourceAccount.ReferPath.Contains(ParentAccountUid.Value.ToString()))
                , AccountId != null);
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<Withdrawal>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long? PartyId { get; set; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long? AccountId { get; set; }

        public CurrencyTypes? CurrencyId { get; set; }
        public FundTypes? FundType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public DateTime? ApprovedFrom { get; set; }
        public DateTime? ApprovedTo { get; set; }
        public StateTypes? StateId { get; set; }

        public List<int>? StateIds { get; set; }

        public PaymentStatusTypes? PaymentStatus { get; set; }

        protected override void OnCollect(ICriteriaPool<Withdrawal> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => x.FundType == (int)FundType!, FundType.IsTangible());
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId.IsTangible());
            pool.Add(x => StateIds!.Contains(x.IdNavigation.StateId), StateIds != null && StateIds.Count != 0);
            pool.Add(x => x.IdNavigation.StatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());
            pool.Add(x => x.Payment.Status == (short)PaymentStatus!, PaymentStatus.HasValue);
            pool.Add(x => AccountId == null ? x.SourceAccountId == null : x.SourceAccountId == AccountId);

            pool.Add(x => x.ApprovedOn >= ApprovedFrom!.Value.ToUniversalTime(), ApprovedFrom.IsTangible());

            pool.Add(x => x.ApprovedOn <= ApprovedTo!.Value.ToUniversalTime(), ApprovedTo.IsTangible());
        }
    }
}