namespace Bacera.Gateway;

using M = PaymentInfo;

partial class PaymentInfo : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public PaymentPlatformTypes? Platform { get; set; }
        public long? PartyId { get; set; }
        public string? Keyword { get; set; }
        public string? InfoKey { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.PaymentPlatform == (int)Platform!, Platform.HasValue && Platform.IsTangible());
            pool.Add(x => x.Name.ToUpper().Contains(Keyword!.ToUpper()), Keyword is { Length: > 2 });
            pool.Add(x => x.Info.Contains(InfoKey!), InfoKey != null);
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

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
        }
    }
}