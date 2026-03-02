using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;

namespace Bacera.Gateway.Services.Report.Models;

public class WithdrawalRecord : ICanExportToCsv
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
    public decimal PaymentAmount { get; set; }

    private decimal ExchangeRate => PaymentAmount == 0 ? 0 : Math.Round(Amount / PaymentAmount, 4);
    
    public DateTime CreatedOn { get; set; }
    public DateTime ApprovedOn { get; set; }

    public static string Header()
        =>
            "account_number,account_uid,client_name,currency,withdrawal_status,payment_status,payment_id,payment_number,payment_method,amount,exchange_rate,created_on,approved_on";

    public string ToCsv()
    {
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var paymentStatusName = Enum.GetName(typeof(PaymentStatusTypes), PaymentStatus) ?? string.Empty;
        var stateName = Enum.GetName(typeof(StateTypes), State) ?? string.Empty;
        var accountNumber = AccountNumber == 0 ? "wallet" : AccountNumber.ToString();

        return
            $"{accountNumber},{AccountUid},{ClientName},{currencyName},{stateName},{paymentStatusName}," +
            $"{PaymentId},{PaymentNumber},{PaymentServiceName},{(Amount / 100m).ToCentsFromScaled()},{ExchangeRate},{CreatedOn.AddHours(2):yyyy-MM-dd HH:mm:ss},{ApprovedOn.AddHours(2):yyyy-MM-dd HH:mm:ss}";
    }
}

public static class WithdrawalRecordExtension
{
    public static IQueryable<WithdrawalRecord> ToRecords(this IQueryable<Withdrawal> query)
        => query.Select(x => new WithdrawalRecord
        {
            AccountNumber = x.SourceAccount != null ? x.SourceAccount.AccountNumber : 0,
            AccountUid = x.SourceAccount != null ? x.SourceAccount.Uid : 0,
            CurrencyId = x.CurrencyId,
            PartyId = x.PartyId,
            State = (StateTypes)x.IdNavigation.StateId,
            PaymentStatus = (PaymentStatusTypes)x.Payment.Status,
            PaymentId = x.Payment.Id,
            PaymentNumber = x.Payment.Number,
            PaymentServiceName = x.Payment.PaymentMethod.Name + " " + x.Payment.ReferenceNumber,
            Amount = x.Amount,
            PaymentAmount = x.Payment.Amount,
            CreatedOn = x.IdNavigation.PostedOn,
            ApprovedOn = x.ApprovedOn,
        });
}