using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;

namespace Bacera.Gateway.Services.Report.Models;

public class WalletOverviewRecord : ICanExportToCsv
{
    public long Id { get; init; }
    public long PartyId { get; init; }
    public string ClientName { get; set; } = string.Empty;
    
    public string? Email { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public FundTypes FundType { get; set; }
    public CurrencyTypes CurrencyId { get; init; }
    public PartyStatusTypes Status { get; set; }


    public static string Header()
        =>
            "id,name,email,currency,fund_type,amount,status";

    public string ToCsv()
    {
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var statusName = Status == PartyStatusTypes.Active ? "Active" : "Closed";

        return
            $"\"{Id}\",\"{ClientName}\",\"{Email}\",\"{currencyName}\",\"{FundType}\",\"{(Amount / 100m).ToCentsFromScaled()}\",\"{statusName}\"";
    }
}

public static class WalletRecordExtension
{
    public static IQueryable<WalletOverviewRecord> ToRecords(this IQueryable<Wallet> query)
        => query.Select(x => new WalletOverviewRecord
        {
            Id = x.Id,
            PartyId = x.PartyId,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            FundType = (FundTypes)x.FundType,
            Amount = x.Balance,
            Status = (PartyStatusTypes)x.Party.Status,
        });

    public static IQueryable<WalletOverviewRecord> ToRecords(this IQueryable<WalletDailySnapshot> query)
        => query.Select(x => new WalletOverviewRecord
        {
            Id = x.Wallet.Id,
            PartyId = x.Wallet.PartyId,
            CurrencyId = (CurrencyTypes)x.Wallet.CurrencyId,
            FundType = (FundTypes)x.Wallet.FundType,
            Amount = x.Balance,
            Status = (PartyStatusTypes)x.Wallet.Party.Status,
        });
}