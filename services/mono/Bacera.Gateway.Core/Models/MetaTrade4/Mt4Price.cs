using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt4Price
{
    public string Symbol { get; set; } = null!;

    public DateTime Time { get; set; }

    public double Bid { get; set; }

    public double Ask { get; set; }

    public double Low { get; set; }

    public double High { get; set; }

    public int Direction { get; set; }

    public int Digits { get; set; }

    public int Spread { get; set; }

    public DateTime ModifyTime { get; set; }
}
