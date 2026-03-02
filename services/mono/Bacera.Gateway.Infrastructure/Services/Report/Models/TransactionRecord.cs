using Bacera.Gateway.Interfaces;

namespace Bacera.Gateway.Services.Report.Models;

public class TransactionRecord : ICanExportToCsv
{
    public long PartyId { get; init; }
    public string ClientName { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public CurrencyTypes CurrencyId { get; init; }
    public StateTypes State { get; set; }
    public long SourceAccountNumber { get; init; }
    public long TargetAccountNumber { get; init; }
    public DateTime CreatedOn { get; set; }


    public static string Header()
        =>
            "client_name,source_account_number,target_account_number,currency,transaction_status,amount,created_on";

    public string ToCsv()
    {
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var stateName = Enum.GetName(typeof(StateTypes), State) ?? string.Empty;
        var sourceAccountNumber = SourceAccountNumber == 0 ? "wallet" : SourceAccountNumber.ToString();
        var targetAccountNumber = TargetAccountNumber == 0 ? "wallet" : TargetAccountNumber.ToString();

        return
            $"\"{ClientName}\",\"{sourceAccountNumber}\",\"{targetAccountNumber}\",\"{currencyName}\",\"{stateName}\"," +
            $"\"{Amount / 100m}\",\"{CreatedOn:yyyy-MM-dd HH:mm:ss}\"";
    }
}