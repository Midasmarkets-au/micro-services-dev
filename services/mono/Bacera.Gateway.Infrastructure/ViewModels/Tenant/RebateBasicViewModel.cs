using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class RebateBasicViewModel
{
    public long Id { get; set; }
    public long Amount { get; set; }
    public long PartyId { get; set; }
    public long? TargetWalletId { get; set; }
    public DateTime PostedOn { get; set; }
    public DateTime ReleasedOn { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public AccountBasicViewModel TargetAccount { get; set; } = AccountBasicViewModel.Empty();


    public TenantUserBasicModel? User { get; set; }
}

public static class RebateBasicViewModelExtension
{
    public static IQueryable<RebateBasicViewModel> ToRebateBasicViewModel(this IQueryable<Rebate> query,
        bool hideEmail = false)
        => query.Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags).Select(x => new RebateBasicViewModel
            {
                Id = x.Id,
                Amount = x.Amount,
                PartyId = x.PartyId,
                PostedOn = x.IdNavigation.PostedOn,
                ReleasedOn = x.IdNavigation.StatedOn,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                TargetWalletId = x.IdNavigation.WalletTransactions.Any() ? x.IdNavigation.WalletTransactions.First().WalletId : null,
                TargetAccount = new AccountBasicViewModel
                {
                    Id = x.AccountId,
                    Uid = x.Account.Uid,
                    Code = x.Account.Code,
                    Group = x.Account.Group,
                    PartyId = x.Account.PartyId,
                    Role = (AccountRoleTypes)x.Account.Role,
                },
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });

    public static IEnumerable<RebateBasicViewModel> ToRebateBasicViewModel(this IEnumerable<Rebate> query)
        => query.Select(x => new RebateBasicViewModel
        {
            Id = x.Id,
            Amount = x.Amount,
            PartyId = x.PartyId,
            PostedOn = x.IdNavigation.PostedOn,
            ReleasedOn = x.IdNavigation.StatedOn,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            TargetWalletId = x.IdNavigation.WalletTransactions.Any() ? x.IdNavigation.WalletTransactions.First().WalletId : null,
            TargetAccount = new AccountBasicViewModel
            {
                Id = x.AccountId,
                Uid = x.Account.Uid,
                Code = x.Account.Code,
                Group = x.Account.Group,
                PartyId = x.Account.PartyId,
                Role = (AccountRoleTypes)x.Account.Role,
            },
            // User = x.Party.ToTenantBasicViewModel(false)
        });
}