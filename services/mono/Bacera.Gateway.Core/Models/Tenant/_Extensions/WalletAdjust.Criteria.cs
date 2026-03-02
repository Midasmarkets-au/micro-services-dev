using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = WalletAdjust;

public partial class WalletAdjust : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public long? WalletId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Email { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Wallet.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.Wallet.Party.Email == Email, Email != null);
            pool.Add(x => x.WalletId == WalletId, WalletId.IsTangible());
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From != null);
            pool.Add(x => x.CreatedOn < To!.Value.ToUniversalTime(), To != null);
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        public StateTypes? StateId { get; set; }

        [JsonIgnore] public long? WalletId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.WalletId == WalletId, WalletId.IsTangible());
            pool.Add(x => x.IdNavigation.StateId == (int)StateId!, StateId != null);
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From != null);
            pool.Add(x => x.CreatedOn < To!.Value.ToUniversalTime(), To != null);
        }
    }
}