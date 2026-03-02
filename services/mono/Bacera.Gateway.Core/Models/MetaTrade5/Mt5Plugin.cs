using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Plugin
{
    public string Name { get; set; } = null!;

    public ulong Server { get; set; }

    public long Timestamp { get; set; }

    public string Module { get; set; } = null!;

    public uint Enable { get; set; }

    public uint Flags { get; set; }
}
