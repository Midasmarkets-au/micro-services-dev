using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

using M = WalletDailySnapshot;

partial class WalletDailySnapshot : IEntity
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public long WalletId { get; set; }
        public long Balance { get; set; }
        public DateTime SnapshotDate { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public FundTypes FundType { get; set; }
        public TenantUserBasicModel User { get; set; } = null!;
    }
}

public static class WalletDailySnapshotExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query, bool hideEmail = false)
        => query
            .Include(x => x.Wallet.Party.PartyComments)
            .Include(x => x.Wallet.Party.Tags)
            .Select(x => new M.TenantPageModel
            {
                Id = x.Id,
                WalletId = x.WalletId,
                Balance = x.Balance,
                SnapshotDate = x.SnapshotDate,
                CurrencyId = (CurrencyTypes)x.Wallet.CurrencyId,
                FundType = (FundTypes)x.Wallet.FundType,
                User = x.Wallet.Party.ToTenantBasicViewModel(hideEmail)
            });
}