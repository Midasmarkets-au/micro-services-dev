using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Bacera.Gateway.SalesRebateSchema;

public partial class SalesRebateSchema : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? SalesAccountUid { get; set; }
        public List<long>? SalesAccountIds { get; set; }
        public long? RebateAccountUid { get; set; }
        public SalesRebateSchemaStatusTypes? Status { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == (short)Status!, Status.HasValue && Status.IsTangible());
            pool.Add(x => SalesAccountIds!.Contains(x.SalesAccountId),
                SalesAccountIds != null && SalesAccountIds.Count != 0 && SalesAccountIds.IsTangible());
            pool.Add(x => x.SalesAccount.Uid == SalesAccountUid, SalesAccountUid.IsTangible());
            pool.Add(x => x.RebateAccount.Uid == RebateAccountUid, RebateAccountUid.IsTangible());
        }
    }
}