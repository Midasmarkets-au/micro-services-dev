using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class TransactionViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long Amount { get; set; }
    public long SourceAccountId { get; set; }
    public long SourceAccountNumber { get; set; }
    public long SourceAccountBalanceInCents { get; set; }
    public double SourceAccountBalance => (double)SourceAccountBalanceInCents / 100;
    public TransactionAccountTypes SourceAccountType { get; set; }
    public bool SourceAccountHasComment { get; set; }
    public long TargetAccountId { get; set; }
    public long TargetAccountNumber { get; set; }
    public long TargetAccountBalanceInCents { get; set; }
    public double TargetAccountBalance => (double)SourceAccountBalanceInCents / 100;
    public TransactionAccountTypes TargetAccountType { get; set; }
    public bool TargetAccountHasComment { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public StateTypes StateId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();
}

public static class TransactionViewModelExtension
{
    public static IQueryable<TransactionViewModel> ToTenantViewModel(this IQueryable<Transaction> query,
        bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.PartyTags)
            .Select(x => new TransactionViewModel
            {
                Id = x.Id,
                PartyId = x.PartyId,
                FundType = (FundTypes)x.FundType,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                StateId = (StateTypes)x.IdNavigation.StateId,
                CreatedOn = x.IdNavigation.PostedOn,
                UpdatedOn = x.IdNavigation.StatedOn,
                Amount = x.Amount,
                SourceAccountId = x.SourceAccountId,
                ReferenceNumber = x.ReferenceNumber,
                SourceAccountType = (TransactionAccountTypes)x.SourceAccountType,
                TargetAccountId = x.TargetAccountId,
                TargetAccountType = (TransactionAccountTypes)x.TargetAccountType,
                OperatorName = x.IdNavigation.Activities.Any() ? x.IdNavigation.Activities.OrderByDescending(a => a.Id).First().Party.NativeName : "",
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}