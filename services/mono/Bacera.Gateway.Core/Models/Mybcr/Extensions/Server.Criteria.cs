namespace Bacera.Gateway;

using M = Server;

public partial class Server : IEntity<ulong>
{
    public sealed class Criteria : EntityCriteria<M, ulong>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public string? Region { get; set; }
        public string? Name { get; set; }
        public string? Stat { get; set; }
        public sbyte? Status { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Region == Region, Region != null);
            pool.Add(x => x.Name.Contains(Name!), Name != null);
            pool.Add(x => x.Stat == Stat, Stat != null);
            pool.Add(x => x.Status == Status, Status != null);
        }
    }
}