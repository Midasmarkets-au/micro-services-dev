using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.Common;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Report.Models;

public class RebateRecord : ICanExportToCsv
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public long Ticket { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public long AccountNumber { get; init; }
    public long AccountUid { get; init; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientCode { get; init; } = string.Empty;
    public int CurrencyId { get; init; }
    public int SourceCurrencyId { get; init; }
    public decimal Volume { get; init; }
    public decimal RebateValue { get; init; }
    private decimal RateValue { get; set; }
    private decimal Rate { get; set; }
    private decimal PipsValue { get; set; }
    private decimal Pips { get; set; }
    private decimal CommissionValue { get; set; }
    private decimal Commission { get; set; }
    public DateTime ClosedOn { get; init; }
    public DateTime ReleasedOn { get; init; }
    public DateTime CreatedOn { get; set; } // (CreatedOn is the time when the rebate is deposited into wallet)
    public string Information { get; init; } = string.Empty;

    /// <summary>
    /// MT5 server offset east of UTC (e.g. 3 for GMT+3). <see cref="ToCsv"/> writes GMT+0 first as wall time minus this value; following columns stay MT5 wall time.
    /// </summary>
    public int MtGmtOffsetHoursForCsv { get; set; } = 2;

    public static string Header() => Header(2);

    public static string Header(int mtGmtOffsetHours) =>
        "Ticket,Symbol,AccountNumber,AccountUid,ClientName,ClientCode,Currency,SourceCurrency,Volume,Amount,RateValue,PipsValue,CommissionValue,Rate,Pips,Commission," +
        $"ClosedOn GMT+0,ClosedOn GMT+{mtGmtOffsetHours},CreatedOn GMT+0,CreatedOn GMT+{mtGmtOffsetHours},ReleasedOn GMT+0,ReleasedOn GMT+{mtGmtOffsetHours}";

    public string ToCsv()
    {
        var obj = JsonConvert.DeserializeObject<dynamic>(Information);

        Rate = (decimal)(obj?.baseRebate?.rate ?? 0);
        Pips = (decimal)(obj?.baseRebate?.pip ?? 0);
        Commission = (decimal)(obj?.baseRebate?.commission ?? 0);

        CommissionValue = (decimal)(obj?.baseRebate?.commission ?? 0);
        RateValue = (decimal)(obj?.baseRebate?.rate ?? 0);
        PipsValue = (decimal)(obj?.baseRebate?.pip ?? 0);
        var currencyName = Enum.GetName(typeof(CurrencyTypes), CurrencyId) ?? string.Empty;
        var sourceCurrencyName = Enum.GetName(typeof(CurrencyTypes), SourceCurrencyId) ?? string.Empty;

        var off = MtGmtOffsetHoursForCsv;
        var co0 = $"{ClosedOn.AddHours(-off):yyyy-MM-dd HH:mm:ss}";
        var co = $"{ClosedOn:yyyy-MM-dd HH:mm:ss}";
        var cr0 = $"{CreatedOn.AddHours(-off):yyyy-MM-dd HH:mm:ss}";
        var cr = $"{CreatedOn:yyyy-MM-dd HH:mm:ss}";
        var rel0 = $"{ReleasedOn.AddHours(-off):yyyy-MM-dd HH:mm:ss}";
        var rel = $"{ReleasedOn:yyyy-MM-dd HH:mm:ss}";
        return
            $"{Ticket},{Symbol},{AccountNumber},{AccountUid},\"{ClientName}\",{ClientCode}," +
            $"{currencyName},{sourceCurrencyName},{Volume / 100m},{(RebateValue / 100m).ToCentsFromScaled()},{RateValue / 100m},{PipsValue / 100m}," +
            $"{CommissionValue / 100m},{Rate / 100m},{Pips / 100m},{Commission / 100m}," +
            $"{co0},{co},{cr0},{cr},{rel0},{rel}";
    }
}

public static class RebateRecordExtensions
{
    public static IQueryable<RebateRecord> ToRecords(this IQueryable<Rebate> query) =>
        query.Select(x => new RebateRecord
        {
            Id = x.Id,
            PartyId = x.PartyId,
            Ticket = x.TradeRebate != null ? x.TradeRebate.Ticket : 0,
            Symbol = x.TradeRebate != null ? x.TradeRebate.Symbol : string.Empty,
            AccountNumber = x.TradeRebate != null ? x.TradeRebate.AccountNumber : 0,
            AccountUid = x.Account.Uid,
            ClientName = x.Account.Name, 
            ClientCode = x.Account.Code,
            CurrencyId = x.CurrencyId,
            // SourceCurrencyId: from TradeRebate.Account.CurrencyId if available, else fallback to Rebate.CurrencyId
            // Note: This requires explicit join in the query, as EF Core ignores .Include() in Select projections
            SourceCurrencyId = x.CurrencyId, // Will be set via explicit join in ProcessRebateRequestAsync
            Volume = x.TradeRebate != null ? x.TradeRebate.Volume : 0,
            RebateValue = x.Amount,
            ClosedOn = x.TradeRebate != null ? x.TradeRebate.ClosedOn : DateTime.MaxValue,
            Information = x.Information,
            ReleasedOn = x.IdNavigation.StatedOn,
        });
}