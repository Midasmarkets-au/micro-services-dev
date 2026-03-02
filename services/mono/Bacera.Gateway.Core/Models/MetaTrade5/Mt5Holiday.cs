using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Holiday
{
    public uint Year { get; set; }

    public uint Month { get; set; }

    public uint Day { get; set; }

    public uint From { get; set; }

    public uint To { get; set; }

    public string Description { get; set; } = null!;

    public long Timestamp { get; set; }

    public uint Mode { get; set; }

    public string Symbols { get; set; } = null!;
}
