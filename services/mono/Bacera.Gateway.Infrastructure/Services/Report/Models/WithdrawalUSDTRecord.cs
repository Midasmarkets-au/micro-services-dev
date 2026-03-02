using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;

namespace Bacera.Gateway.Services.Report.Models;

public class WithdrawalUSDTRecord : ICanExportToCsv
{
    public long AccountNumber { get; init; }
    public long AccountUid { get; init; }
    public long PartyId { get; init; }
    public long PaymentId { get; init; }
    public string ClientName { get; set; } = string.Empty;
    public int CurrencyId { get; init; }
    public StateTypes State { get; set; }
    public PaymentStatusTypes PaymentStatus { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string PaymentServiceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ApprovedOn { get; set; }

    public static string Header()
        =>
            "account_number,account_uid,client_name,currency,withdrawal_status,payment_status,payment_id,payment_number,payment_method,amount,created_on,approved_on";

    public string ToCsv()
    {
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var paymentStatusName = Enum.GetName(typeof(PaymentStatusTypes), PaymentStatus) ?? string.Empty;
        var stateName = Enum.GetName(typeof(StateTypes), State) ?? string.Empty;
        var accountNumber = AccountNumber == 0 ? "wallet" : AccountNumber.ToString();

        return
            $"{accountNumber},{AccountUid},{ClientName},{currencyName},{stateName},{paymentStatusName}," +
            $"{PaymentId},{PaymentNumber},{PaymentServiceName},{(Amount / 100m).ToCentsFromScaled()},{CreatedOn:yyyy-MM-dd HH:mm:ss},{ApprovedOn:yyyy-MM-dd HH:mm:ss}";
    }
}

public static class WithdrawalUSDTRecordExtension
{
    public static IQueryable<WithdrawalUSDTRecord> ToUSDTRecords(this IQueryable<Withdrawal> query)
        => query.Select(x => new WithdrawalUSDTRecord
        {
            AccountNumber = x.SourceAccount != null ? x.SourceAccount.AccountNumber : 0,
            AccountUid = x.SourceAccount != null ? x.SourceAccount.Uid : 0,
            CurrencyId = x.CurrencyId,
            PartyId = x.PartyId,
            State = (StateTypes)x.IdNavigation.StateId,
            PaymentStatus = (PaymentStatusTypes)x.Payment.Status,
            PaymentId = x.Payment.Id,
            PaymentNumber = x.Payment.Number,
            PaymentServiceName = x.Payment.PaymentMethod.Name,
            Amount = x.Amount,
            CreatedOn = x.IdNavigation.PostedOn,
            ApprovedOn = x.ApprovedOn,
        });
}