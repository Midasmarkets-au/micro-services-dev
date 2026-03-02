namespace Bacera.Gateway.Auth;

using M = User;

partial class User : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Withdrawal.Id);
        }

        public long? PartyId { get; set; }
        public long? Uid { get; set; }
        public long? TenantId { get; set; }
        public string? Keywords { get; set; }
        public string? Email { get; set; }

        public List<long>? Uids { get; set; }
        public List<long>? PartyIds { get; set; }
        public bool? IsLocked { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Uid == Uid, Uid.IsTangible());
            pool.Add(x => x.TenantId == TenantId, TenantId != null);
            pool.Add(x => x.Email == Email, Email != null);
            pool.Add(x => Uids!.Contains(x.Uid), Uids != null && Uids.Any());
            pool.Add(x => PartyIds!.Contains(x.PartyId), PartyIds != null && PartyIds.Any());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.LockoutEnabled && x.LockoutEnd > DateTime.UtcNow, IsLocked is true);
            pool.Add(x => x.LockoutEnabled != true || x.LockoutEnd < DateTime.UtcNow, IsLocked is false);
            pool.Add(x =>
                    (x.UserName != null && x.UserName.Contains(Keywords!))
                    || (x.Email != null && x.Email.Contains(Keywords!))
                    || x.FirstName.Contains(Keywords!)
                    || x.LastName.Contains(Keywords!)
                ,
                Keywords is { Length: > 2 });
        }
    }
}