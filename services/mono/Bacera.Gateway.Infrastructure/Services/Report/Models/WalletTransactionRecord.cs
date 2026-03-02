using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;

namespace Bacera.Gateway.Services.Report.Models;

public class WalletTransactionRecord : ICanExportToCsv
{
    public long Id { get; init; } // MatterId
    public long PartyId { get; init; }
    public long WalletId { get; init; }
    public string ClientName { get; set; } = string.Empty;

    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; init; }
    public CurrencyTypes SourceCurrencyId { get; init; }

    public long Source { get; init; }
    public long Target { get; init; }

    public decimal Amount { get; set; }
    public decimal SourceAmount { get; set; }

    public MatterTypes MatterType { get; set; }

    public StateTypes StateId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ReleasedOn { get; set; }

    public long RebateTargetAccountUid { get; init; }

    public static string Header()
        =>
            "wallet_id,transaction_id,client_name,fund_type,transaction_type,source_currency,source,currency,target,state,source_amount,amount,rebate_target_account_uid,created_on,released_on";

    public string ToCsv()
    {
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var sourceCurrencyName = Enum.GetName(typeof(CurrencyTypes), SourceCurrencyId) ?? string.Empty;
        var fundTypeName = Enum.GetName(typeof(FundTypes), FundType) ?? string.Empty;
        var transactionTypeName = Enum.GetName(typeof(MatterTypes), MatterType) ?? string.Empty;
        var stateName = Enum.GetName(typeof(StateTypes), StateId) ?? string.Empty;

        var sourceName = MatterType switch
        {
            MatterTypes.InternalTransfer => Source != 0 ? $"Account No. {Source}" : "Wallet",
            MatterTypes.Rebate => $"Ticket No. {Source}",
            MatterTypes.Deposit => "Deposit Source",
            MatterTypes.Refund => "Refund",
            MatterTypes.WalletAdjust => (WalletAdjustSourceTypes)Source switch
            {
                WalletAdjustSourceTypes.ManualAdjust => "Manual Adjust",
                WalletAdjustSourceTypes.SalesRebate => "Sales Rebate",
                _ => string.Empty
            },
            MatterTypes.System => "System",
            _ => string.Empty
        };

        var targetName = MatterType switch
        {
            MatterTypes.InternalTransfer => Target != 0 ? $"Account No. {Target}" : "Wallet",
            MatterTypes.Withdrawal => "Withdrawal Target",
            MatterTypes.Refund => "Wallet",
            MatterTypes.WalletAdjust => "Wallet",
            MatterTypes.System => "System",
            _ => string.Empty
        };

        var rebateTargetAccountName = MatterType switch
        {
            MatterTypes.Rebate => RebateTargetAccountUid != 0 ? $"{RebateTargetAccountUid}" : "",
            _ => string.Empty
        };

        return
            $"\"{WalletId}\",\"{Id}\",\"{ClientName}\",\"{fundTypeName}\",\"{transactionTypeName}\",\"{sourceCurrencyName}\",\"{sourceName}\",\"{currencyName}\",\"{targetName}\"," +
            $"\"{stateName}\",\"{(SourceAmount / 100).ToCentsFromScaled()}\",\"{(Amount / 100).ToCentsFromScaled()}\",\"{rebateTargetAccountName}\",\"{CreatedOn:yyyy-MM-dd HH:mm:ss}\",\"{ReleasedOn:yyyy-MM-dd HH:mm:ss}\"";
    }
}