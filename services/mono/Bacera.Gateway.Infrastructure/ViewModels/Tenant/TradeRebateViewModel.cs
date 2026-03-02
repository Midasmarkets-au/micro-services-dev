using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class TradeRebateViewModel
{
    public bool IsEmpty() => Id == 0;
    public long Id { get; set; }

    public int TradeServiceId { get; set; }

    public long Ticket { get; set; }

    public CurrencyTypes CurrencyId { get; set; }

    public int Volume { get; set; }

    public string Symbol { get; set; } = null!;

    public TradeRebateStatusTypes Status { get; set; }

    public RebateRuleTypes RuleType { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public DateTime ClosedOn { get; set; }

    public DateTime OpenedOn { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public ICollection<long> RebateIds { get; set; } = new List<long>();

    public AccountBasicViewModel SourceTradeAccount { get; set; } = AccountBasicViewModel.Empty();

    public List<RebateBasicViewModel> Rebates { get; set; } = new();
}

public static class TradeRebateViewModelExtension
{
    public static IQueryable<TradeRebateViewModel> ToTenantViewModel(this IQueryable<TradeRebate> query,
        bool hideEmail = false) =>
        query
            .Include(x => x.Account!.Party.PartyComments)
            .Include(x => x.Account!.Party.Tags)
            .Select(x => new TradeRebateViewModel
            {
                Id = x.Id,
                TradeServiceId = x.TradeServiceId,
                Ticket = x.Ticket,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                Volume = x.Volume,
                Symbol = x.Symbol,
                Status = (TradeRebateStatusTypes)x.Status,
                RuleType = (RebateRuleTypes)x.RuleType,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                ClosedOn = x.ClosedOn,
                OpenedOn = x.OpenedOn,
                RebateIds = x.Rebates.Select(r => r.Id).ToList(),
                SourceTradeAccount = x.AccountId != null
                    ? new AccountBasicViewModel
                    {
                        Id = x.Account!.Id,
                        Uid = x.Account.Uid,
                        PartyId = x.Account.PartyId,
                        Role = (AccountRoleTypes)x.Account.Role,
                        Name = x.Account.Name,
                        ReferPath = x.Account.ReferPath,
                        Code = x.Account.Role == (short)AccountRoleTypes.Sales
                            ? x.Account.Code
                            : x.Account.SalesAccount != null
                                ? x.Account.SalesAccount.Code
                                : string.Empty,
                        Group = x.Account.Group,
                        AccountNumber = x.AccountNumber,
                        User = x.Account.Party.ToTenantBasicViewModel(hideEmail)
                    }
                    : new AccountBasicViewModel()
            });
}