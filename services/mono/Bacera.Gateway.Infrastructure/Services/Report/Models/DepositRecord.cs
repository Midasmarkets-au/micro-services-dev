using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;
using Newtonsoft.Json;
using Twilio.Rest.Api.V2010.Account.Call;

namespace Bacera.Gateway.Services.Report.Models;

public class DepositRecord : ICanExportToCsv
{
    public long AccountNumber { get; init; }
    public long AccountUid { get; init; }
    public long PartyId { get; init; }
    public string ClientName { get; set; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int CurrencyId { get; init; }
    public StateTypes State { get; set; }
    public PaymentStatusTypes PaymentStatus { get; set; }
    public long PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string PaymentServiceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }

    public static string Header()
        =>
            "account_number,account_uid,client_name,currency,deposit_status,payment_status,payment_id,payment_number,payment_method,amount,created_on";

    public string ToCsv()
    {
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var paymentStatusName = Enum.GetName(typeof(PaymentStatusTypes), PaymentStatus) ?? string.Empty;
        var stateName = Enum.GetName(typeof(StateTypes), State) ?? string.Empty;
        var accountNumber = AccountNumber == 0 ? "wallet" : AccountNumber.ToString();

        return
            $"\"{accountNumber}\",\"{AccountUid}\",\"{ClientName}\",\"{currencyName}\",\"{stateName}\",\"{paymentStatusName}\"," +
            $"\"{PaymentId}\",{PaymentNumber}\",\"{PaymentServiceName}\",\"{(Amount / 100m).ToCentsFromScaled()}\",\"{CreatedOn.AddHours(2):yyyy-MM-dd HH:mm:ss}\"";
    }
}

public static class DepositRecordExtension
{
    public static IQueryable<DepositRecord> ToRecords(this IQueryable<Deposit> query)
        => query.Select(x => new DepositRecord
        {
            AccountNumber = x.TargetAccount != null ? x.TargetAccount.AccountNumber : 0,
            AccountUid = x.TargetAccount != null ? x.TargetAccount.Uid : 0,
            CurrencyId = x.CurrencyId,
            PartyId = x.PartyId,
            State = (StateTypes)x.IdNavigation.StateId,
            PaymentStatus = (PaymentStatusTypes)x.Payment.Status,
            PaymentId = x.PaymentId,
            PaymentNumber = x.Payment.Number,
            PaymentServiceName = x.Payment.PaymentMethod.Name + " " + x.Payment.ReferenceNumber,
            Amount = x.Amount,
            CreatedOn = x.IdNavigation.PostedOn,
        });
}