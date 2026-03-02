namespace Bacera.Gateway;

using M = AccountReportGroup;

public partial class AccountReportGroup : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? GroupId { get; set; }
        public string? Group { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;

        public long? AccountNumber { get; set; }

        public List<long>? AccountNumbers { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.ParentId == GroupId || x.Id == GroupId, GroupId != null);
            pool.Add(x => x.Group.ToLower() == Group!.ToLower(), !string.IsNullOrEmpty(Group));
            pool.Add(x => x.Category.ToLower() == Category!.ToLower(), !string.IsNullOrEmpty(Category));
            pool.Add(x => x.AccountReportGroupLogins.Any(y => y.Login == AccountNumber), AccountNumber != null);
            pool.Add(x => x.AccountReportGroupLogins.Any(y => AccountNumbers!.Contains(y.Login)),
                AccountNumbers != null);
        }
    }
}