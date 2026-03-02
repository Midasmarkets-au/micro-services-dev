using Bacera.Gateway.Interfaces;

namespace Bacera.Gateway.ViewModels.Tenant;

public class DepositViewModelForAgent
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long PartyId { get; set; }

    public int Type { get; set; }
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long Amount { get; set; }
    public long? TargetTradeAccountId { get; set; }
    public AccountBasicViewModel TargetTradeAccount { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public StateTypes StateId { get; set; }
    public bool HasReceipt { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public ParentUserBasicModel User { get; set; } = ParentUserBasicModel.Empty();
}

public static class DepositViewModelForAgentExtension
{
    public static IQueryable<DepositViewModelForAgent> ToParentViewModel(this IQueryable<Deposit> query)
        => query.Select(x => new DepositViewModelForAgent
        {
            PartyId = x.PartyId,
            Type = x.Type,
            Amount = x.Amount,
            FundType = (FundTypes)x.FundType,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            StateId = (StateTypes)x.IdNavigation.StateId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.PostedOn,
            TargetTradeAccountId = x.TargetAccountId,
            ReferenceNumber = x.ReferenceNumber,
            TargetTradeAccount = new AccountBasicViewModel
            {
                Id = x.TargetAccount != null ? x.TargetAccount.Id : 0L,
                Uid = x.TargetAccount != null ? x.TargetAccount.Uid : 0L,
                AccountNumber = x.TargetAccount != null ? x.TargetAccount.AccountNumber : 0L,
                PartyId = x.TargetAccount != null ? x.TargetAccount.PartyId : 0L,
                CurrencyId = x.TargetAccount != null
                    ? (CurrencyTypes)x.TargetAccount.CurrencyId
                    : (CurrencyTypes)(-1),
                Group = x.TargetAccount != null ? x.TargetAccount.Group : string.Empty,
                Code = x.TargetAccount != null && x.TargetAccount.SalesAccount != null
                    ? x.TargetAccount.SalesAccount.Code
                    : string.Empty,
            },
            User = x.Party.ToParentBasicViewModel()
        });
}