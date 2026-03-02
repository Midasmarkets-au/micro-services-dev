using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Position
{
    public ulong PositionId { get; set; }

    public ulong Position { get; set; }

    public long Timestamp { get; set; }

    public string ExternalId { get; set; } = null!;

    public ulong Login { get; set; }

    public ulong Dealer { get; set; }

    public string Symbol { get; set; } = null!;

    public uint Action { get; set; }

    public uint Digits { get; set; }

    public uint DigitsCurrency { get; set; }

    public uint Reason { get; set; }

    public double ContractSize { get; set; }

    public DateTime TimeCreate { get; set; }

    public DateTime TimeUpdate { get; set; }

    public DateTime TimeCreateMsc { get; set; }

    public DateTime TimeUpdateMsc { get; set; }

    public double PriceOpen { get; set; }

    public double PriceCurrent { get; set; }

    public double PriceSl { get; set; }

    public double PriceTp { get; set; }

    public ulong VolumeExt { get; set; }

    public double Profit { get; set; }

    public double Storage { get; set; }

    public double RateProfit { get; set; }

    public double RateMargin { get; set; }

    public ulong ExpertId { get; set; }

    public ulong ExpertPositionId { get; set; }

    public string Comment { get; set; } = null!;

    public uint ActivationMode { get; set; }

    public long ActivationTime { get; set; }

    public double ActivationPrice { get; set; }

    public uint ActivationFlags { get; set; }

    public ulong Volume { get; set; }

    public string ApiData { get; set; } = null!;
}
