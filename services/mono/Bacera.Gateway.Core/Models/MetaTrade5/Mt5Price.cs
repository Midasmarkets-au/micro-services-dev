using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Price
{
    public ulong PriceId { get; set; }

    public string Symbol { get; set; } = null!;

    public uint Digits { get; set; }

    public uint BidDir { get; set; }

    public uint AskDir { get; set; }

    public uint LastDir { get; set; }

    public DateTime Time { get; set; }

    public double BidLast { get; set; }

    public double BidHigh { get; set; }

    public double BidLow { get; set; }

    public double AskLast { get; set; }

    public double AskHigh { get; set; }

    public double AskLow { get; set; }

    public double LastLast { get; set; }

    public double LastHigh { get; set; }

    public double LastLow { get; set; }
}
