using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Order
{
    public ulong Order { get; set; }

    public long Timestamp { get; set; }

    public string ExternalId { get; set; } = null!;

    public ulong Login { get; set; }

    public ulong Dealer { get; set; }

    public string Symbol { get; set; } = null!;

    public uint Digits { get; set; }

    public uint DigitsCurrency { get; set; }

    public double ContractSize { get; set; }

    public uint State { get; set; }

    public uint Reason { get; set; }

    public DateTime TimeSetup { get; set; }

    public DateTime TimeExpiration { get; set; }

    public DateTime TimeDone { get; set; }

    public DateTime TimeSetupMsc { get; set; }

    public DateTime TimeDoneMsc { get; set; }

    public uint ModifyFlags { get; set; }

    public uint Type { get; set; }

    public uint TypeFill { get; set; }

    public uint TypeTime { get; set; }

    public double PriceOrder { get; set; }

    public double PriceTrigger { get; set; }

    public double PriceCurrent { get; set; }

    public double PriceSl { get; set; }

    public double PriceTp { get; set; }

    public ulong VolumeInitialExt { get; set; }

    public ulong VolumeCurrentExt { get; set; }

    public ulong ExpertId { get; set; }

    public ulong PositionId { get; set; }

    public ulong PositionById { get; set; }

    public double RateMargin { get; set; }

    public string Comment { get; set; } = null!;

    public uint ActivationMode { get; set; }

    public long ActivationTime { get; set; }

    public double ActivationPrice { get; set; }

    public uint ActivationFlags { get; set; }

    public ulong VolumeInitial { get; set; }

    public ulong VolumeCurrent { get; set; }

    public string ApiData { get; set; } = null!;
}
