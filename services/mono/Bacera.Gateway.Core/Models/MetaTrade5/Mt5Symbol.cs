using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Symbol
{
    public ulong SymbolId { get; set; }

    public string Symbol { get; set; } = null!;

    public long Timestamp { get; set; }

    public string Path { get; set; } = null!;

    public string Isin { get; set; } = null!;

    public string Cfi { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Exchange { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string International { get; set; } = null!;

    public uint Sector { get; set; }

    public uint Industry { get; set; }

    public string Country { get; set; } = null!;

    public string Basis { get; set; } = null!;

    public string Source { get; set; } = null!;

    public string Page { get; set; } = null!;

    public string CurrencyBase { get; set; } = null!;

    public uint CurrencyBaseDigits { get; set; }

    public string CurrencyProfit { get; set; } = null!;

    public uint CurrencyProfitDigits { get; set; }

    public string CurrencyMargin { get; set; } = null!;

    public uint CurrencyMarginDigits { get; set; }

    public uint Color { get; set; }

    public uint ColorBackground { get; set; }

    public uint Digits { get; set; }

    public double Point { get; set; }

    public double Multiply { get; set; }

    public ulong TickFlags { get; set; }

    public uint TickBookDepth { get; set; }

    public uint TickChartMode { get; set; }

    public uint SubscriptionsDelay { get; set; }

    public uint FilterSoft { get; set; }

    public uint FilterSoftTicks { get; set; }

    public uint FilterHard { get; set; }

    public uint FilterHardTicks { get; set; }

    public uint FilterDiscard { get; set; }

    public uint FilterSpreadMax { get; set; }

    public uint FilterSpreadMin { get; set; }

    public uint FilterGap { get; set; }

    public uint FilterGapTicks { get; set; }

    public uint TradeMode { get; set; }

    public ulong TradeFlags { get; set; }

    public uint CalcMode { get; set; }

    public uint ExecMode { get; set; }

    public uint Gtcmode { get; set; }

    public uint FillFlags { get; set; }

    public uint ExpirFlags { get; set; }

    public uint OrderFlags { get; set; }

    public int Spread { get; set; }

    public int SpreadBalance { get; set; }

    public int SpreadDiff { get; set; }

    public int SpreadDiffBalance { get; set; }

    public double TickValue { get; set; }

    public double TickSize { get; set; }

    public double ContractSize { get; set; }

    public int StopsLevel { get; set; }

    public int FreezeLevel { get; set; }

    public uint QuotesTimeout { get; set; }

    public ulong VolumeMinExt { get; set; }

    public ulong VolumeMaxExt { get; set; }

    public ulong VolumeStepExt { get; set; }

    public ulong VolumeLimitExt { get; set; }

    public uint MarginFlags { get; set; }

    public double MarginInitial { get; set; }

    public double MarginMaintenance { get; set; }

    public double MarginInitialBuy { get; set; }

    public double MarginInitialSell { get; set; }

    public double MarginInitialBuyLimit { get; set; }

    public double MarginInitialSellLimit { get; set; }

    public double MarginInitialBuyStop { get; set; }

    public double MarginInitialSellStop { get; set; }

    public double MarginInitialBuyStopLimit { get; set; }

    public double MarginInitialSellStopLimit { get; set; }

    public double MarginMaintenanceBuy { get; set; }

    public double MarginMaintenanceSell { get; set; }

    public double MarginMaintenanceBuyLimit { get; set; }

    public double MarginMaintenanceSellLimit { get; set; }

    public double MarginMaintenanceBuyStop { get; set; }

    public double MarginMaintenanceSellStop { get; set; }

    public double MarginMaintenanceBuyStopLimit { get; set; }

    public double MarginMaintenanceSellStopLimit { get; set; }

    public double MarginRateLiquidity { get; set; }

    public double MarginHedged { get; set; }

    public double MarginRateCurrency { get; set; }

    public uint SwapMode { get; set; }

    public double SwapLong { get; set; }

    public double SwapShort { get; set; }

    public uint? SwapFlags { get; set; }

    public uint? SwapYearDay { get; set; }

    public double? SwapRateSunday { get; set; }

    public double? SwapRateMonday { get; set; }

    public double? SwapRateTuesday { get; set; }

    public double? SwapRateWednesday { get; set; }

    public double? SwapRateThursday { get; set; }

    public double? SwapRateFriday { get; set; }

    public double? SwapRateSaturday { get; set; }

    public DateTime TimeStart { get; set; }

    public DateTime TimeExpiration { get; set; }

    public uint Reflags { get; set; }

    public uint Retimeout { get; set; }

    public uint IecheckMode { get; set; }

    public uint Ietimeout { get; set; }

    public uint IeslipProfit { get; set; }

    public uint IeslipLosing { get; set; }

    public ulong IevolumeMaxExt { get; set; }

    public double PriceSettle { get; set; }

    public double PriceLimitMax { get; set; }

    public double PriceLimitMin { get; set; }

    public double PriceStrike { get; set; }

    public uint OptionMode { get; set; }

    public double FaceValue { get; set; }

    public double AccruedInterest { get; set; }

    public uint SpliceType { get; set; }

    public uint SpliceTimeType { get; set; }

    public uint SpliceTimeDays { get; set; }

    public ulong? VolumeMin { get; set; }

    public ulong? VolumeMax { get; set; }

    public ulong? VolumeStep { get; set; }

    public ulong? VolumeLimit { get; set; }

    public ulong? IevolumeMax { get; set; }
}
