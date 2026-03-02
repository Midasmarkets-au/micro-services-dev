using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5NetworkAccessServer
{
    public ulong Login { get; set; }

    public uint Priority { get; set; }

    public uint AntifloodEnable { get; set; }

    public uint AntifloodConnects { get; set; }

    public uint AntifloodErrors { get; set; }

    public uint NewsMaxCount { get; set; }

    public uint BalancingConnections { get; set; }

    public uint BalancingPriority { get; set; }

    public uint AccessMask { get; set; }

    public uint AccessFlags { get; set; }

    public string Servers { get; set; } = null!;
}
