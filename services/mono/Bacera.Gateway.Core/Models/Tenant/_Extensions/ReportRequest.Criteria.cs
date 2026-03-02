namespace Bacera.Gateway;

using M = ReportRequest;

partial class ReportRequest : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            // Sorting is handled in the controller to properly handle NULL GeneratedOn values
        }
        public long? PartyId { get; set; }
        public int? Type { get; set; }
        public string? Keywords { get; set; }
        public bool? IsGenerated { get; set; }
        public bool? IsExpired { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == Type!, Type.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.GeneratedOn != null, IsGenerated is true);
            pool.Add(x => x.GeneratedOn == null, IsGenerated is false);
            pool.Add(x => x.ExpireOn < DateTime.UtcNow, IsExpired is true);
            pool.Add(x => x.ExpireOn > DateTime.UtcNow, IsExpired is false);
            pool.Add(x => x.Name.Contains(Keywords!), Keywords is { Length: > 2 });
        }
    }
}