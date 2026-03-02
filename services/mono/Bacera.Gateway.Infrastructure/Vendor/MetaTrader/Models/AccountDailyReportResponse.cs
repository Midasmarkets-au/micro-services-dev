using Bacera.Gateway.Services.Common;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

// public class AccountDailyReportResponse
// {
//     [JsonProperty("retcode")] public string Retcode { get; set; } = string.Empty;
//     [JsonProperty("answer")] public List<Answer> Answer { get; set; } = new();
// }

public class AccountDailyReportResponse
{
    public long Timestamp { get; set; }

    public DateTime ReportDate => AccountDailyReportResponseExt.LdapToDateTime(Timestamp);

    public string DatetimePrev { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string CurrencyDigits { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public double Balance { get; set; }
    public double Credit { get; set; }
    public double InterestRate { get; set; }
    public double CommissionDaily { get; set; }
    public double CommissionMonthly { get; set; }
    public double AgentDaily { get; set; }
    public double AgentMonthly { get; set; }
    public double BalancePrevDay { get; set; }
    public double BalancePrevMonth { get; set; }
    public double EquityPrevDay { get; set; }
    public double EquityPrevMonth { get; set; }
    public double Margin { get; set; }
    public double MarginFree { get; set; }
    public double MarginLevel { get; set; }
    public int MarginLeverage { get; set; }
    public double Profit { get; set; }
    public double ProfitStorage { get; set; }
    public double ProfitEquity { get; set; }
    public double ProfitAssets { get; set; }
    public double ProfitLiabilities { get; set; }
    public double DailyProfit { get; set; }
    public double DailyBalance { get; set; }
    public double DailyCredit { get; set; }
    public double DailyCharge { get; set; }
    public double DailyCorrection { get; set; }
    public double DailyBonus { get; set; }
    public double DailyStorage { get; set; }
    public double DailyCommInstant { get; set; }
    public double DailyCommFee { get; set; }
    public double DailyCommRound { get; set; }
    public double DailyAgent { get; set; }
    public double DailyInterest { get; set; }
    public double DailyDividend { get; set; }
    public double DailyTaxes { get; set; }

    [JsonProperty("DailySOCompensation")] public double DailySoCompensation { get; set; }

    [JsonProperty("DailySOCompensationCredit")]
    public double DailySoCompensationCredit { get; set; }

    public List<PositionDetail> Positions { get; set; } = new();
    public List<OrderDetails> Orders { get; set; } = new();
}

public class PositionDetail
{
    [JsonProperty("Position")] public long PositionId { get; set; }
    [JsonProperty("ExternalID")] public string ExternalId { get; set; } = string.Empty;
    public long Login { get; set; }
    public string Dealer { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int Action { get; set; }
    public int Digits { get; set; }
    public int DigitsCurrency { get; set; }
    public int Reason { get; set; }
    public double ContractSize { get; set; }
    public long TimeCreate { get; set; }
    public long TimeUpdate { get; set; }
    public long TimeCreateMsc { get; set; }
    public long TimeUpdateMsc { get; set; }
    public int ModifyFlags { get; set; }
    public double PriceOpen { get; set; }
    public double PriceCurrent { get; set; }
    [JsonProperty("PriceSL")] public double PriceSl { get; set; }
    [JsonProperty("PriceTP")] public double PriceTp { get; set; }
    public int Volume { get; set; }
    public long VolumeExt { get; set; }
    public double Profit { get; set; }
    public double Storage { get; set; }
    public double RateProfit { get; set; }
    public double RateMargin { get; set; }
    [JsonProperty("ExpertID")] public string ExpertId { get; set; } = string.Empty;
    [JsonProperty("ExpertPositionID")] public string ExpertPositionId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int ActivationMode { get; set; }
    public long ActivationTime { get; set; }
    public double ActivationPrice { get; set; }
    public int ActivationFlags { get; set; }

    [JsonIgnore] public List<ApiData> ApiData { get; set; } = new();
}

public class OrderDetails
{
    public long Order { get; set; }
    [JsonProperty("ExternalID")] public string ExternalId { get; set; } = string.Empty;
    public long Login { get; set; }
    public string Dealer { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int Digits { get; set; }
    public int DigitsCurrency { get; set; }
    public double ContractSize { get; set; }
    public int State { get; set; }
    public int Reason { get; set; }
    public long TimeSetup { get; set; }
    public long TimeExpiration { get; set; }
    public long TimeDone { get; set; }
    public long TimeSetupMsc { get; set; }
    public long TimeDoneMsc { get; set; }
    public int ModifyFlags { get; set; }
    public int Type { get; set; }
    public int TypeFill { get; set; }
    public int TypeTime { get; set; }
    public double PriceOrder { get; set; }
    public double PriceTrigger { get; set; }
    public double PriceCurrent { get; set; }
    [JsonProperty("PriceSL")] public double PriceSl { get; set; }
    [JsonProperty("PriceTP")] public double PriceTp { get; set; }
    public int VolumeInitial { get; set; }
    public long VolumeInitialExt { get; set; }
    public int VolumeCurrent { get; set; }
    public long VolumeCurrentExt { get; set; }
    [JsonProperty("ExpertID")] public string ExpertId { get; set; } = string.Empty;
    [JsonProperty("PositionID")] public long PositionId { get; set; }
    [JsonProperty("PositionByID")] public string PositionById { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public double RateMargin { get; set; }
    public int ActivationMode { get; set; }
    public long ActivationTime { get; set; }
    public double ActivationPrice { get; set; }
    public int ActivationFlags { get; set; }
    [JsonIgnore] public List<ApiData> ApiData { get; set; } = new();

    private DateTime UtcMscToDateTime(long utcMsc)
        => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(utcMsc);
}

public class ApiData
{
    [JsonProperty("AppID")] public int AppId { get; set; }
    [JsonProperty("ID")] public int Id { get; set; }
    public int ValueInt { get; set; }
    public uint ValueUInt { get; set; }
    public double ValueDouble { get; set; }
}

public static class AccountDailyReportResponseExt
{
    public static DateTime UtcMscToDateTime(long utcMsc)
        => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(utcMsc);

    public static DateTime LdapToDateTime(long ladpTime)
        => new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(ladpTime);

    public static IEnumerable<TradeViewModel> ToViewModel(this IEnumerable<PositionDetail> list, int? serviceId = null)
        => list.Select(x => new TradeViewModel
        {
            Id = x.PositionId,
            Position = x.PositionId,
            Symbol = x.Symbol,
            Cmd = x.Action,
            Digits = x.Digits,
            Ticket = x.PositionId,
            Comment = x.Comment,
            Profit = x.Profit,
            Reason = x.Reason,
            Volume = x.Volume.ToCentsFromScaled(),
            AccountNumber = x.Login,
            OpenAt = UtcMscToDateTime(x.TimeCreateMsc),
            OpenPrice = x.PriceOpen,
            CurrentPrice = x.PriceCurrent,
            CloseAt = null,
            ClosePrice = null,
            ServiceId = serviceId ?? 0,
            TimeStamp = x.TimeCreate,
            CreatedOn = UtcMscToDateTime(x.TimeCreateMsc),
            UpdatedOn = UtcMscToDateTime(x.TimeUpdateMsc),
            Sl = x.PriceSl,
            Tp = x.PriceTp,
            Commission = 0,
            Swaps = x.Storage,
        });

    // public static IEnumerable<TradeViewModel> ToViewModel(this IEnumerable<OrderDetails> list, int? serviceId = null)
    //     => list.Select(x => new TradeViewModel
    //     {
    //         Id = x.Order,
    //         Position = x.PositionId,
    //         Symbol = x.Symbol,
    //         Cmd = (int)x.Action,
    //         Digits = x.Digits,
    //         Ticket = x.PositionId,
    //         Comment = x.Comment,
    //         Profit = x.Profit,
    //         Reason = x.Reason,
    //         Volume = x.VolumeClosed / 10000.0,
    //         AccountNumber = x.Login,
    //         OpenAt = x.VolumeClosed == 0 ? x.Time : null,
    //         OpenPrice = x.VolumeClosed == 0 ? x.Price : 0,
    //         CloseAt = x.VolumeClosed > 0 ? x.Time : null,
    //         ClosePrice = x.PriceCurrent,
    //         ServiceId = serviceId ?? 0,
    //         TimeStamp = x.TimeSetup,
    //         CreatedOn = UtcMscToDateTime(x.TimeSetupMsc),
    //         UpdatedOn = UtcMscToDateTime(x.TimeSetupMsc),
    //         Sl = x.PriceSl,
    //         Tp = x.PriceTp,
    //         Commission = x.Commission,
    //         Swaps = x.Storage,
    //     });
}