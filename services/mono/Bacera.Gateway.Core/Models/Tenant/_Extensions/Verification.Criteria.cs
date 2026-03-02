namespace Bacera.Gateway;

using M = Verification;

partial class Verification : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public string? Email { get; set; }
        public string? SearchText { get; set; }
        public List<int>? Statuses { get; set; }
        public VerificationTypes? Type { get; set; }
        public VerificationStatusTypes? Status { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId.Equals(PartyId), PartyId.IsTangible());
            pool.Add(x => x.Type == (short)Type!, Type != null && Type.IsTangible());
            pool.Add(x => x.Status == (int)Status!, Status != null && Status.IsTangible());
            pool.Add(x => x.Party.Email == Email!, Email != null);
            pool.Add(x => Statuses!.Contains(x.Status), Statuses != null && Statuses.Any());
            pool.Add(x => x.Party.SearchText.Contains(SearchText!), SearchText != null);
        }
    }
}