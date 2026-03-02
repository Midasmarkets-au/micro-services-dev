using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Report.Models;

public class SalesRebateRecord : ICanExportToCsv
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public long TradeRebateId { get; set; }

    public long SalesAccountUid { get; set; }

    public decimal Amount { get; set; }

    public short TradeAccountType { get; set; }

    public int TradeAccountCurrencyId { get; set; }

    public short Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public long TradeAccountNumber { get; set; }

    public string RebateType { get; set; } = null!;

    public long RebateBase { get; set; }

    public long? WalletId { get; set; }

    public static string Header() =>
        "id,trade_rebate_id,sales_uid,trade_account_number,account_type,currency,amount,status,rebate_type,rebate_base,wallet_id,created_on";

    public string ToCsv()
    {
        var statusName = Enum.GetName(typeof(SalesRebateStatusTypes), Status) ?? string.Empty;
        var currencyName = Enum.GetName(typeof(CurrencyTypes), TradeAccountCurrencyId) ?? string.Empty;

        return
            $"{Id},{TradeRebateId},{SalesAccountUid},{TradeAccountNumber},{TradeAccountType},{currencyName}," +
            $"{Amount / 100m},{statusName},{RebateType},{RebateBase},{WalletId},{CreatedOn.AddHours(2):yyyy-MM-dd HH:mm:ss}";
    }
}

public class SalesRebateSumByAccountRecord : ICanExportToCsv
{
    public long Amount { get; set; }
    public long PartyId { get; set; }
    public long WalletId { get; set; }
    public long SalesCode { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public long TradeAccountNumber { get; set; }

    public static string Header() =>
        "trade_account_number,total_amount,wallet_id,sales_code";

    public string ToCsv()
    {
        return
            $"{TradeAccountNumber},{Amount / 100m},{WalletId},{SalesCode}";
    }
}

public static class SalesRebateRecordExtensions
{
    public static IQueryable<SalesRebateRecord> ToRecords(this IQueryable<SalesRebate> query) =>
        query.Select(x => new SalesRebateRecord
        {
            Id = x.Id,
            TradeRebateId = x.TradeRebateId,
            SalesAccountUid = x.SalesAccount.Uid,
            Amount = x.Amount,
            TradeAccountType = x.TradeAccountType,
            TradeAccountCurrencyId = x.TradeAccountCurrencyId,
            Status = x.Status,
            CreatedOn = x.CreatedOn,
            TradeAccountNumber = x.TradeAccountNumber,
            RebateType = x.RebateType,
            RebateBase = x.RebateBase,
            WalletId = x.WalletAdjust == null ? 0 : x.WalletAdjust.WalletId,
        });
}