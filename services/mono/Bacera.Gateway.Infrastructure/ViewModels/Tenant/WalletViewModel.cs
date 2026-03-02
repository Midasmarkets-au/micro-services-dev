using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class WalletViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long Balance { get; set; }
    public string Code { get; set; } = string.Empty;
    public long SalesAccountUid { get; set; }
    public TenantUserBasicModel User { get; set; } = new();
}

public static class WalletViewModelExtension
{
    public static IQueryable<WalletViewModel> ToTenantViewModel(this IQueryable<Wallet> query, bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Select(x => new
            {
                Wallet = x,
                FirstAccount = x.Party.Accounts.FirstOrDefault(y => y.IsClosed == 0 && y.Status == (short)AccountStatusTypes.Activate),
                User = x.Party.ToTenantBasicViewModel(hideEmail),
            })
            .Select(x => new WalletViewModel
            {
                Id = x.Wallet.Id,
                PartyId = x.Wallet.PartyId,
                FundType = (FundTypes)x.Wallet.FundType,
                CurrencyId = (CurrencyTypes)x.Wallet.CurrencyId,
                Balance = x.Wallet.Balance,
                Code = x.FirstAccount != null
                    ? x.FirstAccount.Role != (short)AccountRoleTypes.Sales
                        ? x.FirstAccount.SalesAccount != null
                            ? x.FirstAccount.SalesAccount!.Code
                            : ""
                        : x.FirstAccount.Code
                    : "",
                SalesAccountUid = x.FirstAccount != null
                    ? x.FirstAccount.Role != (short)AccountRoleTypes.Sales
                        ? x.FirstAccount.SalesAccount != null ? x.FirstAccount.SalesAccount!.Uid : 0
                        : x.FirstAccount.Uid
                    : 0,
                User = x.User,
                IsPrimary = x.Wallet.IsPrimary == 1, // Crossing different fund types, one party has one primary wallet
            });
}