using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Bacera.Gateway.Configuration;

public partial class Configuration : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public string? Category { get; set; }

        public List<long>? RowIds { get; set; }
        public List<string>? Keys { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => RowIds!.Contains(x.RowId), RowIds != null && RowIds.Count != 0);
            pool.Add(x => Keys!.Contains(x.Key), Keys != null && Keys.Count != 0);
            pool.Add(x => x.Category.ToUpper() == Category!.ToUpper(), Category != null);
        }
    }
}