using System.Text;

namespace Bacera.Gateway;

using M = Party;

partial class Party : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(CreatedOn);
            // SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public long? Uid { get; set; }
        public string? SearchText { get; set; }
        public string? Email { get; set; }

        public List<long>? Uids { get; set; }
        public List<long>? PartyIds { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Uid == Uid, Uid.IsTangible());
            pool.Add(x => x.Email == Email, Email != null);
            pool.Add(x => Uids!.Contains(x.Uid), Uids != null && Uids.Any());
            pool.Add(x => PartyIds!.Contains(x.Id), PartyIds != null && PartyIds.Any());
            pool.Add(x => x.Id == PartyId, PartyId.IsTangible());
            pool.Add(x => x.SearchText.ToLower().Contains(SearchText!.ToLower()), SearchText != null);
        }
    }
}