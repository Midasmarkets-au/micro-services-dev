using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Refund;

partial class Refund : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Withdrawal.Id);
        }

        public long? PartyId { get; set; }

        public RefundTargetTypes? TargetType { get; set; }
        public long? TargetId { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public FundTypes? FundType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Email { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.TargetType == (int)TargetType!, TargetType != null);
            pool.Add(x => x.TargetId == TargetId, TargetId != null);
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId != null);
            pool.Add(x => x.FundType == (int)FundType!, FundType != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);
            pool.Add(x => x.Party.Email == Email!, Email != null);
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Withdrawal.Id);
        }

        [JsonIgnore] public long? PartyId { get; set; }

        [JsonIgnore] public RefundTargetTypes? TargetType { get; set; }

        [JsonIgnore] public long? TargetId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.TargetType == (int)TargetType!, TargetType != null);
            pool.Add(x => x.TargetId == TargetId, TargetId != null);
            pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From != null);
            pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To != null);
        }
    }
}