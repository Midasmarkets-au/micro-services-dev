using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class DepositViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public int Type { get; set; }
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long Amount { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public long? TargetTradeAccountId { get; set; }
    public AccountBasicViewModel TargetTradeAccount { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public StateTypes StateId { get; set; }
    public bool HasReceipt { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;

    public List<string> PartyTags { get; set; } = new();
    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();
    public PaymentBasicViewModel Payment { get; set; } = PaymentBasicViewModel.Empty();
}

public static class DepositViewModelExtension
{
    public static IQueryable<DepositViewModel> ToTenantViewModel(this IQueryable<Deposit> query, bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Select(x => new DepositViewModel
            {
                Id = x.Id,
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
                    Code = x.TargetAccount != null && x.TargetAccount.SalesAccount != null
                        ? x.TargetAccount.SalesAccount.Code
                        : "",
                    Group = x.TargetAccount != null
                        ? x.TargetAccount.Role != (short)AccountRoleTypes.Agent
                            ? x.TargetAccount.Group
                            : x.TargetAccount.AgentAccount != null
                                ? x.TargetAccount.AgentAccount.Group
                                : ""
                        : "",
                    HasComment = x.TargetAccount != null
                        ? x.TargetAccount.AccountComments.Count != 0
                        : null,
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
                    ExchangeRate = (x.Payment.Amount / (decimal)x.Amount).ToString("0.000"),
                    PaymentServiceId = x.Payment.PaymentServiceId,
                    PaymentServiceName =
                        x.Payment.CryptoTransaction != null ? x.Payment.CryptoTransaction.Crypto.Name : x.Payment.PaymentMethod.Name,
                    PaymentServicePlatformId = (PaymentPlatformTypes)x.Payment.PaymentMethod.Platform,
                },
                // User = new TenantUserBasicModel
                // {
                //     Id = 0,
                //     PartyId = x.PartyId,
                //     Email = x.Party.Email,
                //     Avatar = x.Party.Avatar,
                //     FirstName = x.Party.FirstName,
                //     LastName = x.Party.LastName,
                //     NativeName = x.Party.NativeName,
                //     PartyTags = x.Party.Tags.Select(t => t.Name).ToList(),
                //     HasComment = x.Party.PartyComments.Count != 0,
                // },
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}