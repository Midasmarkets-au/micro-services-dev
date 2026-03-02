namespace Bacera.Gateway;

using M = Bacera.Gateway.UserBlackList;

public partial class UserBlackList
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string IdNumber { get; set; } = "";
        public string OperatorName { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}

public static class UserBlackListExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query)
        => query.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            Name = x.Name,
            Phone = x.Phone,
            Email = x.Email,
            IdNumber = x.IdNumber,
            OperatorName = x.OperatorName,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });
}