namespace Bacera.Gateway;

using M = Deposit;

partial class Deposit : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            // SortField = nameof(Withdrawal.IdNavigation.PostedOn);
        }

        public static Criteria BuildForPeriod(DateTime? from, DateTime? to) => new()
        {
            From = from,
            To = to,
            StateId = StateTypes.DepositCompleted,
        };

        public long? PartyId { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public StateTypes? StateId { get; set; }
        public long? ParentAccountUid { get; set; }
        public string? Email { get; set; }
        public string? Target { get; set; }
        public long? TotalAmount { get; set; }
        public long? AccountUid { get; set; }
        public long? AccountId { get; set; }
        public long? AccountNumber { get; set; }
        public List<int>? StateIds { get; set; }
        public AccountRoleTypes? Role { get; set; }

        public PaymentStatusTypes? PaymentStateId { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId.IsTangible());
            pool.Add(x => StateIds!.Contains(x.IdNavigation.StateId), StateIds != null && StateIds.Any());
            pool.Add(x => x.IdNavigation.StatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.IdNavigation.StatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());

            pool.Add(x => x.Payment.Status == (short)PaymentStateId!, PaymentStateId.HasValue);

            pool.Add(x => x.Party.Email == Email!, Email != null);
            pool.Add(x => x.Party.Accounts.Any(a => a.Status == 0 && a.Role == (int)Role!), Role != null);

            pool.Add(x => x.TargetAccount != null
                          && x.TargetAccount.ReferPath.Contains(ParentAccountUid!.Value.ToString()),
                ParentAccountUid != null);

            pool.Add(x => x.TargetAccount != null
                          && x.TargetAccount.Uid == AccountUid
                          && (ParentAccountUid == null
                              || x.TargetAccount.ReferPath.Contains(ParentAccountUid.Value.ToString()))
                , AccountUid != null);

            pool.Add(x => x.TargetAccount != null
                          && x.TargetAccount.AccountNumber == AccountNumber
                          && (ParentAccountUid == null
                              || x.TargetAccount.ReferPath.Contains(ParentAccountUid.Value.ToString()))
                , AccountNumber != null);

            pool.Add(x => x.TargetAccountId != null
                          && x.TargetAccount != null
                          && x.TargetAccountId == AccountId
                          && (ParentAccountUid == null
                              || x.TargetAccount.ReferPath.Contains(ParentAccountUid.Value.ToString()))
                , AccountId != null);
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
            pool.Add(x => x.Payment.Status == (short)PaymentStatus!, PaymentStatus.HasValue);
            pool.Add(x => x.TargetAccountId == AccountId, AccountId != null);
        }
    }
}