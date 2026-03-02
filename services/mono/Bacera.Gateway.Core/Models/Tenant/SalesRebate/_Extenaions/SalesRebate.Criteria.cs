using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Bacera.Gateway.SalesRebate;

public partial class SalesRebate : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? TradeRebateId { get; set; }
        public long? Ticket { get; set; }
        public long? TradeAccountUid { get; set; }
        public long? TradeAccountNumber { get; set; }
        public long? SalesAccountUid { get; set; }

        public bool? IsFromDirectClient { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        
        public SalesRebateStatusTypes? Status { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.TradeRebateId == TradeRebateId, TradeRebateId.IsTangible());
            pool.Add(x => x.SalesAccount.Uid == SalesAccountUid, SalesAccountUid.IsTangible());
            pool.Add(x => x.TradeAccount.Uid == TradeAccountUid, TradeAccountUid.IsTangible());
            pool.Add(x => x.TradeRebate.Ticket == Ticket, Ticket.IsTangible());
            pool.Add(x => x.TradeAccountNumber == TradeAccountNumber, TradeAccountNumber.IsTangible());
            pool.Add(x => x.TradeAccount.SalesAccountId != null && x.TradeAccount.SalesAccountId == x.SalesAccountId, IsFromDirectClient == true);
            pool.Add(x => x.Status == (short)Status!, Status.HasValue);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);
            //pool.Add(x => x.CreatedOn >= From!.Value.ToUniversalTime(), From != null);
            //pool.Add(x => x.CreatedOn <= To!.Value.ToUniversalTime(), To != null);
        }
    }
}