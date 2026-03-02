using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Routing
{
    public string Name { get; set; } = null!;

    public long Timestamp { get; set; }

    public uint Mode { get; set; }

    public uint Request { get; set; }

    public uint Type { get; set; }

    public uint Flags { get; set; }

    public uint Action { get; set; }

    public string? ActionValueString { get; set; }

    public long? ActionValueInt { get; set; }

    public ulong? ActionValueUint { get; set; }

    public double? ActionValueFloat { get; set; }

    public uint ActionType { get; set; }
}
