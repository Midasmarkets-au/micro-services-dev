namespace Bacera.Gateway;

using M = Group;

partial class Group : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public long? PartyId { get; set; }
        public long? AccountId { get; set; }
        public string? Keywords { get; set; }
        public AccountGroupTypes? Type { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Type == (int)Type!, Type.IsTangible());
            pool.Add(x => x.OwnerAccountId == AccountId, AccountId.IsTangible());
            pool.Add(x => x.OwnerAccount.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.Name.Contains(Keywords!), Keywords is { Length: > 2 });
        }
    }
}