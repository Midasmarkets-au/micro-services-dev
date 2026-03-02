using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Deals2025
{
    public ulong Deal { get; set; }

    public long Timestamp { get; set; }

    public string ExternalId { get; set; } = null!;

    public ulong Login { get; set; }

    public ulong Dealer { get; set; }

    public ulong Order { get; set; }

    public uint Action { get; set; }

    public uint Entry { get; set; }

    public uint Reason { get; set; }

    public uint Digits { get; set; }

    public uint DigitsCurrency { get; set; }

    public double ContractSize { get; set; }

    public DateTime Time { get; set; }

    public DateTime TimeMsc { get; set; }

    public string Symbol { get; set; } = null!;

    public double Price { get; set; }

    public ulong VolumeExt { get; set; }

    public double Profit { get; set; }

    public double Storage { get; set; }

    public double Commission { get; set; }

    public double Fee { get; set; }

    public double RateProfit { get; set; }

    public double RateMargin { get; set; }

    public ulong ExpertId { get; set; }

    public ulong PositionId { get; set; }

    public string Comment { get; set; } = null!;

    public double ProfitRaw { get; set; }

    public double PricePosition { get; set; }

    public double PriceSl { get; set; }

    public double PriceTp { get; set; }

    public ulong VolumeClosedExt { get; set; }

    public double TickValue { get; set; }

    public double TickSize { get; set; }

    public ulong Flags { get; set; }

    public string Gateway { get; set; } = null!;

    public double PriceGateway { get; set; }

    public uint ModifyFlags { get; set; }

    public double MarketBid { get; set; }

    public double MarketAsk { get; set; }

    public double MarketLast { get; set; }

    public ulong Volume { get; set; }

    public ulong VolumeClosed { get; set; }

    public string ApiData { get; set; } = null!;

    public double Value { get; set; }

    public uint VolumeGatewayExt { get; set; } = 0;

    public int ActionGateway { get; set; } = 0;
}