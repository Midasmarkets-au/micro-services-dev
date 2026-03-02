namespace Bacera.Gateway;

using M = Medium;

public partial class Medium : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public string? Guid { get; set; }
        public long? TenantId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Type { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.TenantId == TenantId, TenantId.HasValue && TenantId.IsTangible());
            pool.Add(x => x.Guid.ToUpper().Equals(Guid!.ToUpper()), !string.IsNullOrEmpty(Guid));
            pool.Add(x => x.Type.ToUpper().Equals(Type!.ToUpper()), !string.IsNullOrEmpty(Type));
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public long? PartyId { get; set; }

        public string? Guid { get; set; }
        public long? TenantId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Type { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.TenantId == TenantId, TenantId.HasValue && TenantId.IsTangible());
            pool.Add(x => x.Guid.ToUpper().Equals(Guid!.ToUpper()), !string.IsNullOrEmpty(Guid));
            pool.Add(x => x.Type.ToUpper().Equals(Type!.ToUpper()), !string.IsNullOrEmpty(Type));
        }
    }
}