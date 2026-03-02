namespace Bacera.Gateway;

using M = ReportRequest;

partial class ReportRequest
{
    public class ClientPageModel
    {
        public int Type { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public bool? IsGenerated { get; set; }
        public bool? IsExpired { get; set; }
        public string FileName { get; set; } = null!;
    }
}

public static class ReportRequestViewModelExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q) => q.Select(x => new M.ClientPageModel
    {
        Type = x.Type,
        Name = x.Name,
        CreatedOn = x.CreatedOn,
        IsGenerated = x.IsGenerated,
        IsExpired = x.IsExpired,
        FileName = x.FileName
    });
}