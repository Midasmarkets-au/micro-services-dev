namespace Bacera.Gateway;

using M = ContactRequest;

partial class ContactRequest : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public string? Keyword { get; set; }
        public bool? IsArchived { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To.IsTangible());
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From.IsTangible());
            pool.Add(x => x.IsArchived == 1, IsArchived is true);
            pool.Add(x => x.IsArchived == 0, IsArchived is false);
            pool.Add(x => x.Name.Contains(Keyword!) || x.Email.Contains(Keyword!) || x.PhoneNumber.Contains(Keyword!), !string.IsNullOrEmpty(Keyword));
        }
    }
}