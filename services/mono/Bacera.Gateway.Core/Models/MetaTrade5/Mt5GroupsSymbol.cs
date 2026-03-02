using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5GroupsSymbol
{
    public ulong SymbolId { get; set; }

    public ulong GroupId { get; set; }

    public string Path { get; set; } = null!;

    public long Timestamp { get; set; }

    public uint? TradeMode { get; set; }

    public uint? ExecMode { get; set; }

    public uint? FillFlags { get; set; }

    public uint? ExpirFlags { get; set; }

    public uint? OrderFlags { get; set; }

    public int? SpreadDiff { get; set; }

    public int? SpreadDiffBalance { get; set; }

    public int? StopsLevel { get; set; }

    public int? FreezeLevel { get; set; }

    public ulong? VolumeMinExt { get; set; }

    public ulong? VolumeMaxExt { get; set; }

    public ulong? VolumeStepExt { get; set; }

    public ulong? VolumeLimitExt { get; set; }

    public uint? MarginFlags { get; set; }

    public double? MarginInitial { get; set; }

    public double? MarginMaintenance { get; set; }

    public double? MarginInitialBuy { get; set; }

    public double? MarginInitialSell { get; set; }

    public double? MarginInitialBuyLimit { get; set; }

    public double? MarginInitialSellLimit { get; set; }

    public double? MarginInitialBuyStop { get; set; }

    public double? MarginInitialSellStop { get; set; }

    public double? MarginInitialBuyStopLimit { get; set; }

    public double? MarginInitialSellStopLimit { get; set; }

    public double? MarginMaintenanceBuy { get; set; }

    public double? MarginMaintenanceSell { get; set; }

    public double? MarginMaintenanceBuyLimit { get; set; }

    public double? MarginMaintenanceSellLimit { get; set; }

    public double? MarginMaintenanceBuyStop { get; set; }

    public double? MarginMaintenanceSellStop { get; set; }

    public double? MarginMaintenanceBuyStopLimit { get; set; }

    public double? MarginMaintenanceSellStopLimit { get; set; }

    public double? MarginLiquidity { get; set; }

    public double? MarginHedged { get; set; }

    public double? MarginCurrency { get; set; }

    public uint? SwapMode { get; set; }

    public double? SwapLong { get; set; }

    public double? SwapShort { get; set; }

    public uint? SwapFlags { get; set; }

    public uint? SwapYearDay { get; set; }

    public double? SwapRateSunday { get; set; }

    public double? SwapRateMonday { get; set; }

    public double? SwapRateTuesday { get; set; }

    public double? SwapRateWednesday { get; set; }

    public double? SwapRateThursday { get; set; }

    public double? SwapRateFriday { get; set; }

    public double? SwapRateSaturday { get; set; }

    public uint? Reflags { get; set; }

    public uint? Retimeout { get; set; }

    public uint? Ieflags { get; set; }

    public uint? IecheckMode { get; set; }

    public uint? Ietimeout { get; set; }

    public uint? IeslipProfit { get; set; }

    public uint? IeslipLosing { get; set; }

    public ulong? IevolumeMaxExt { get; set; }

    public uint? PermissionsFlags { get; set; }

    public uint? PermissionsBookdepth { get; set; }

    public ulong? VolumeMin { get; set; }

    public ulong? VolumeMax { get; set; }

    public ulong? VolumeStep { get; set; }

    public ulong? VolumeLimit { get; set; }

    public ulong? IevolumeMax { get; set; }
}
