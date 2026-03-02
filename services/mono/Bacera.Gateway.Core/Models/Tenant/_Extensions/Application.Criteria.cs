namespace Bacera.Gateway;

using M = Application;

partial class Application : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public long? ReferenceId { get; set; }
        public ApplicationTypes? Type { get; set; }
        public List<short>? Types { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Email { get; set; }

        public ApplicationStatusTypes? Status { get; set; }
        public List<short>? Statuses { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (short)Type!, Type.IsTangible());
            pool.Add(x => Types!.Contains(x.Type), Types != null && Types.Any());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.UpdatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.UpdatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());
            pool.Add(x => x.ReferenceId == ReferenceId, ReferenceId.IsTangible());
            pool.Add(x => x.Status == (short)Status!, Status.HasValue && Status.IsTangible());
            pool.Add(x => Statuses!.Contains(x.Status), Statuses != null && Statuses.Any());
            pool.Add(x => x.Party.Email == Email, Email != null);
        }
    }
}