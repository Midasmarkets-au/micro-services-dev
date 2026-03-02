using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Report
{
    public string Name { get; set; } = null!;

    public ulong Server { get; set; }

    public long Timestamp { get; set; }

    public string Template { get; set; } = null!;

    public uint Enable { get; set; }
}
