namespace Bacera.Gateway;

using M = WalletAdjust;

public partial class WalletAdjust
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public long WalletId { get; set; }
        public short SourceType { get; set; }
        public long Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Comment { get; set; } = "";
        public string NativeName { get; set; } = "";
        public string OperatorName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsPrimary { get; set; } = false;
    }

    public sealed class ClientPageModel
    {
        public short SourceType { get; set; }
        public long Amount { get; set; }
        public StateTypes StateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Comment { get; set; } = "";
    }
}

public static class WalletAdjustExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query)
        => query.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            WalletId = x.WalletId,
            SourceType = x.SourceType,
            Amount = x.Amount,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Comment = x.Comment,
            NativeName = x.Wallet.Party.NativeName,
            OperatorName = x.IdNavigation.Activities.Any() ? x.IdNavigation.Activities.First().Party.NativeName : "",
            Email = x.Wallet.Party.Email,
            IsPrimary = x.Wallet.IsPrimary == 1
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            SourceType = x.SourceType,
            Amount = x.Amount,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Comment = x.Comment,
            StateId = (StateTypes)x.IdNavigation.StateId
        });
}