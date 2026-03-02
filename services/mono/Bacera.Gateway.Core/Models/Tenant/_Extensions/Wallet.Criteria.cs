namespace Bacera.Gateway;

using M = Wallet;

partial class Wallet : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Sequence);
        }

        public long? PartyId { get; set; }
        public long? AccountUid { get; set; }
        public FundTypes? FundType { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }
        public string? Email { get; set; }
        public bool? HasBalance { get; set; }
        public AccountRoleTypes? Role { get; set; }
        public List<AccountRoleTypes>? Roles { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.TalliedOn <= UpdatedTo!.Value.ToUniversalTime(), UpdatedTo.IsTangible());
            pool.Add(x => x.TalliedOn >= UpdatedFrom!.Value.ToUniversalTime(), UpdatedFrom.IsTangible());
            pool.Add(x => x.FundType == (int)FundType!, FundType.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => (bool)HasBalance! ? x.Balance > 0 || x.Balance < 0 : x.Balance == 0,
                HasBalance != null && HasBalance.IsTangible());
            pool.Add(x => x.Party.Accounts.Any(a => (AccountRoleTypes)a.Role == Role), Role.IsTangible());
            pool.Add(x => x.Party.Email == Email!, Email != null);
            pool.Add(x => x.Party.Accounts.Any(a => Roles!.Contains((AccountRoleTypes)a.Role)), Roles.IsTangible());
            pool.Add(x => x.Party.Accounts.Any(a => a.Uid == AccountUid), AccountUid != null);
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Sequence);
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public long? PartyId { get; set; }
        public long? AccountUid { get; set; }
        public FundTypes? FundType { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public bool? HasBalance { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.FundType == (int)FundType!, FundType.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.IsTangible());
            pool.Add(x => (bool)HasBalance! ? x.Balance > 0 || x.Balance < 0 : x.Balance == 0,
                HasBalance != null && HasBalance.IsTangible());
            pool.Add(x => x.Party.Accounts.Any(a => a.Uid == AccountUid), AccountUid != null);
        }
    }
}