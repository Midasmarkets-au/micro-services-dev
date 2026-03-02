using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Bacera.Gateway.Case;

public partial class Case : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public long? AdminPartyId { get; set; }
        public bool? Claimed { get; set; }
        public CaseStatusTypes? Status { get; set; }
        public IEnumerable<CaseStatusTypes>? Statuses { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool? IncludeReply { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.AdminPartyId == null && x.ReplyId == null, Claimed is false);
            pool.Add(x => x.AdminPartyId != null, Claimed is true);
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.AdminPartyId == AdminPartyId, AdminPartyId != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((CaseStatusTypes)x.Status)!, Statuses != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);
            pool.Add(x => x.ReplyId == null, IncludeReply is not true);
        }
    }
}