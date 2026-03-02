using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using M = Bacera.Gateway.IpBlackList;

public partial class IpBlackList : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
            SortFlag = true;
        }
        public string? OperatorName { get; set; }
        public string? Ip { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.OperatorName.Contains(OperatorName!), OperatorName is { Length: > 2 });
            pool.Add(x => x.Ip.Contains(Ip!), Ip is { Length: > 2 });
        }
    }
}