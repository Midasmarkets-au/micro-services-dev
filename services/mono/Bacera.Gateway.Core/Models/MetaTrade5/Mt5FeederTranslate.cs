using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5FeederTranslate
{
    public string Symbol { get; set; } = null!;

    public string Feeder { get; set; } = null!;

    public string Source { get; set; } = null!;

    public int BidMarkup { get; set; }

    public int AskMarkup { get; set; }

    public uint Digits { get; set; }
}
