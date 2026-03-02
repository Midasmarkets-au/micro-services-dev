using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class RebateViewModel : RebateBasicViewModel
{
    public TradeRebate.SummaryResponseModel Trade { get; set; } = new();
}

public static class RebateViewModelExt
{
    public static IQueryable<RebateViewModel>
        ToTenantViewModel(this IQueryable<Rebate> query, bool hideEmail = false) =>
        query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Include(x => x.Account.Party.PartyComments)
            .Include(x => x.Account.Party.Tags)
            .Select(x => new RebateViewModel
            {
                PartyId = x.PartyId,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                Amount = x.Amount,
                TargetAccount = new AccountBasicViewModel
                {
                    Id = x.AccountId,
                    Uid = x.Account.Uid,
                    Code = x.Account.Code,
                    Group = x.Account.Group,
                    PartyId = x.Account.PartyId,
                    Role = (AccountRoleTypes)x.Account.Role,
                    User = x.Account.Party.ToTenantBasicViewModel(hideEmail),
                },
                PostedOn = x.IdNavigation.PostedOn,
                Trade = new TradeRebate.SummaryResponseModel
                {
                    Volume = x.TradeRebate != null ? x.TradeRebate.Volume : 0,
                    Ticket = x.TradeRebate != null ? x.TradeRebate.Ticket : 0,
                    Symbol = x.TradeRebate != null ? x.TradeRebate.Symbol : string.Empty,
                },
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}