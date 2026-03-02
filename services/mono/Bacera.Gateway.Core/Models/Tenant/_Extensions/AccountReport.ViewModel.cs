namespace Bacera.Gateway;

using M = AccountReport;

partial class AccountReport
{
    public class ClientPageModel
    {
        public long AccountNumber { get; set; }
        public string Date { get; set; } = null!;
        public string DataFile { get; set; } = null!;
        public short Status { get; set; }
        public int Type { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}

public static class AccountReportViewModelExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q) => q.Select(x => new M.ClientPageModel
    {
        AccountNumber = x.AccountNumber,
        Date = x.Date.ToString("yyyy-MM"),
        DataFile = x.DataFile,
        Status = x.Status,
        Type = x.Type,
        CreatedOn = x.CreatedOn,
    });
}