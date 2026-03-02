using Bacera.Gateway.Interfaces;

namespace Bacera.Gateway.ViewModels.Tenant;

public class WithdrawalViewModelForAgent
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long PartyId { get; set; }

    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public StateTypes StateId { get; set; }
    public decimal ExchangeRate { get; set; }
    public WithdrawalSourceViewModel Source { get; set; } = new WithdrawalSourceViewModel();
    public ParentUserBasicModel User { get; set; } = ParentUserBasicModel.Empty();

    public static WithdrawalViewModelForAgent Empty() => new();
}

public static class WithdrawalViewModelForAgentExtension
{
    public static IQueryable<WithdrawalViewModelForAgent> ToParentViewModel(this IQueryable<Withdrawal> query)
        => query.Select(x => new WithdrawalViewModelForAgent
        {
            PartyId = x.PartyId,
            FundType = (FundTypes)x.FundType,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            StateId = (StateTypes)x.IdNavigation.StateId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.PostedOn,
            Amount = x.Amount,
            ExchangeRate = x.ExchangeRate,
            Source = x.SourceAccountId > 0
                ? new WithdrawalSourceViewModel
                {
                    AccountType = TransactionAccountTypes.Account,
                    DisplayNumber = x.SourceAccount!.AccountNumber.ToString(),
                    BalanceInCents = x.SourceAccount.TradeAccountStatus != null
                        ? x.SourceAccount.TradeAccountStatus.BalanceInCents
                        : 0,
                    SalesGroupName = x.SourceAccount != null && x.SourceAccount.SalesAccount != null
                        ? x.SourceAccount.SalesAccount.Code
                        : string.Empty,
                    AgentGroupName = x.SourceAccount != null ? x.SourceAccount.Group : string.Empty,
                    CurrencyId = (CurrencyTypes)x.SourceAccount!.CurrencyId
                }
                : new WithdrawalSourceViewModel
                {
                    AccountType = TransactionAccountTypes.Wallet,
                    BalanceInCents =
                        x.Party.Wallets.First(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType)
                            .BalanceInCents,
                    CurrencyId = (CurrencyTypes)x.CurrencyId
                },
            User = x.Party.ToParentBasicViewModel()
        });
}