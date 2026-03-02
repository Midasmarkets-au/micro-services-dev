using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class WithdrawalViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public StateTypes StateId { get; set; }
    public decimal ExchangeRate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public WithdrawalSourceViewModel Source { get; set; } = new WithdrawalSourceViewModel();
    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();
    public PaymentBasicViewModel Payment { get; set; } = PaymentBasicViewModel.Empty();
    public static WithdrawalViewModel Empty() => new();
}

public class WithdrawalSourceViewModel
{
    public long Id { get; set; }
    public TransactionAccountTypes AccountType { get; set; }
    public string DisplayNumber { get; set; } = string.Empty;
    public double BalanceInCents { get; set; }
    public double Balance => BalanceInCents / 100d;
    public double EquityInCents { get; set; }
    public double Equity => EquityInCents / 100d;
    public string SalesGroupName { get; set; } = string.Empty;
    public string AgentGroupName { get; set; } = string.Empty;
    public CurrencyTypes CurrencyId { get; set; }
    public bool HasComment { get; set; }
}

public static class WithdrawalViewModelExtension
{
    public static IQueryable<WithdrawalViewModel> ToTenantViewModel(this IQueryable<Withdrawal> query,
        bool hideEmail = false)
        => query.Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Select(x => new WithdrawalViewModel
            {
                Id = x.Id,
                PartyId = x.PartyId,
                FundType = (FundTypes)x.FundType,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                StateId = (StateTypes)x.IdNavigation.StateId,
                CreatedOn = x.IdNavigation.PostedOn,
                UpdatedOn = x.IdNavigation.PostedOn,
                Amount = x.Amount,
                ReferenceNumber = x.ReferenceNumber,
                ExchangeRate = x.ExchangeRate,
                Source = x.SourceAccountId != null
                    ? new WithdrawalSourceViewModel
                    {
                        Id = x.SourceAccountId.Value,
                        AccountType = TransactionAccountTypes.Account,
                        DisplayNumber = x.SourceAccount!.AccountNumber.ToString(),
                        BalanceInCents = x.SourceAccount.TradeAccountStatus != null
                            ? x.SourceAccount.TradeAccountStatus.BalanceInCents
                            : 0,
                        EquityInCents = x.SourceAccount.TradeAccountStatus != null
                            ? x.SourceAccount.TradeAccountStatus.EquityInCents
                            : 0,
                        SalesGroupName =
                            x.SourceAccount!.SalesAccount != null
                                ? x.SourceAccount.SalesAccount.Code
                                : string.Empty,
                        AgentGroupName =
                            x.SourceAccount!.AgentAccount != null
                                ? x.SourceAccount.AgentAccount.Group
                                : string.Empty,
                        CurrencyId = (CurrencyTypes)x.SourceAccount!.CurrencyId,
                        HasComment = x.SourceAccount.AccountComments.Count != 0
                    }
                    : new WithdrawalSourceViewModel
                    {
                        AccountType = TransactionAccountTypes.Wallet,
                        BalanceInCents =
                            x.Party.Wallets.First(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType)
                                .BalanceInCents.ToCentsFromScaled(),
                        DisplayNumber = x.Party.Wallets.First(w => w.CurrencyId == x.CurrencyId && w.FundType == x.FundType)
                            .Id.ToString(),
                        CurrencyId = (CurrencyTypes)x.CurrencyId
                    },
                Payment = new PaymentBasicViewModel
                {
                    Id = x.PaymentId,
                    Pid = x.Payment.Pid,
                    PartyId = x.Payment.PartyId,
                    Amount = x.Payment.Amount,
                    Status = (PaymentStatusTypes)x.Payment.Status,
                    CurrencyId = (CurrencyTypes)x.Payment.CurrencyId,
                    LedgerSide = x.Payment.LedgerSide,
                    CreatedOn = x.Payment.CreatedOn,
                    UpdatedOn = x.Payment.UpdatedOn,
                    Number = x.Payment.Number,
                    ReferenceNumber = x.Payment.ReferenceNumber,

                    PaymentServiceId = x.Payment.PaymentServiceId,
                    PaymentServiceName = x.Payment.PaymentMethod.Name,
                    PaymentServicePlatformId = (PaymentPlatformTypes)x.Payment.PaymentMethod.Platform,
                },
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}