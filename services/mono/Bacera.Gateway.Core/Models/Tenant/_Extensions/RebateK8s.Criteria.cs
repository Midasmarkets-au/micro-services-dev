using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class RebateK8s : IEntity
{
    public sealed class Criteria : EntityCriteria<RebateK8s>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        public long? PartyId { get; set; }
        public long? AccountId { get; set; }
        public long? AccountUid { get; set; }
        public long? TicketNumber { get; set; }
        public long? AccountNumber { get; set; }
        public string? Symbol { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public StateTypes? StateId { get; set; }
        public List<StateTypes>? StateIds { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Email { get; set; }

        public long? TotalAmount { get; set; }
        public long? TotalVolume { get; set; }
        public long? PageTotalAmount { get; set; }
        public long? PageTotalVolume { get; set; }

        /// <summary>
        /// RebateK8s has no EF navigation to TradeRebateK8s (composite-keyed, partitioned table).
        /// When Symbol/AccountNumber/TicketNumber filters are set, the caller (service layer) must
        /// resolve matching trade_rebate_k8s ids first and assign them here before PagedFilterBy runs.
        /// </summary>
        [JsonIgnore]
        public List<long>? MatchedTradeRebateIds { get; set; }

        public bool NeedsTradeRebateLookup => Symbol.IsTangible() || AccountNumber.IsTangible() || TicketNumber.IsTangible();

        protected override void OnCollect(ICriteriaPool<RebateK8s> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.AccountId == AccountId, AccountId.IsTangible());
            pool.Add(x => x.Account.Uid == AccountUid, AccountUid.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue);
            pool.Add(x => x.Matter.StateId == (int)StateId!, StateId.HasValue);
            pool.Add(x => StateIds!.Contains((StateTypes)x.Matter.StateId), StateIds != null && StateIds.Any());
            pool.Add(x => x.Matter.StatedOn >= From, From.IsTangible());
            pool.Add(x => x.Matter.StatedOn < To, To.IsTangible());
            pool.Add(x => x.Party.Email == Email, Email.IsTangible());

            pool.Add(x => x.TradeRebateId != null && MatchedTradeRebateIds!.Contains(x.TradeRebateId.Value),
                MatchedTradeRebateIds != null);
        }

        protected override IQueryable<RebateK8s> Pagination(IQueryable<RebateK8s> source)
        {
            try
            {
                if (Page < 1 && Size < 1) return source;
                Total = source.Count();
                Page = Page < 1 ? 1 : Page;
                Size = Size < 1 ? 20 : Size;
                PageCount = (int)Math.Ceiling(Total / (decimal)Size);
                HasMore = PageCount > Page;
                return source.Skip((Page - 1) * Size).Take(Size);
            }
            catch
            {
                return source;
            }
        }
    }
}
