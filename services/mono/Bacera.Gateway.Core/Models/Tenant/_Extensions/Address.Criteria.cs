using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Address;

public partial class Address : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public bool? IncludeDeleted { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.DeletedOn == null, IncludeDeleted is not true);
        }
    }
    
    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(M.Id);
        }
        [JsonIgnore]
        public long? PartyId { get; set; }
        public bool? IncludeDeleted { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.DeletedOn == null, IncludeDeleted is not true);
        }
    }
}