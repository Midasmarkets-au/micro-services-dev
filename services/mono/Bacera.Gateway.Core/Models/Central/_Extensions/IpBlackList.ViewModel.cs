namespace Bacera.Gateway;

using M = Bacera.Gateway.IpBlackList;

public partial class IpBlackList
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string Ip { get; set; } = null!;
        public bool Enabled { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string OperatorName { get; set; } = null!;
        public string Note { get; set; } = null!;
    }
}

public static class IpBlackListViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query)
        => query.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            Ip = x.Ip,
            Enabled = x.Enabled,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            OperatorName = x.OperatorName,
            Note = x.Note,
        });
}