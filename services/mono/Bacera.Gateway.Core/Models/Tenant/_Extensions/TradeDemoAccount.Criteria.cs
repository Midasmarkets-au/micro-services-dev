namespace Bacera.Gateway;

using M = TradeDemoAccount;

partial class TradeDemoAccount : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public int? ServiceId { get; set; }
        public long? AccountNumber { get; set; }

        public bool? IncludeExpired { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId.Equals(PartyId), PartyId.IsTangible());
            pool.Add(x => x.ServiceId == ServiceId, ServiceId.IsTangible());
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.IsTangible());
            pool.Add(x => x.ExpireOn > DateTime.UtcNow, IncludeExpired is not true);
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(ExpireOn);
        }

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public long? PartyId { get; set; }

        public int? ServiceId { get; set; }
        public long? AccountNumber { get; set; }

        public bool? IncludeExpired { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId.Equals(PartyId), PartyId.IsTangible());
            pool.Add(x => x.ServiceId == ServiceId, ServiceId.IsTangible());
            pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.IsTangible());
            pool.Add(x => x.ExpireOn > DateTime.UtcNow, IncludeExpired is not true);
        }
    }
}