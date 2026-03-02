using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5RoutingCond
{
    public ulong ConditionId { get; set; }

    public string RoutingName { get; set; } = null!;

    public uint Condition { get; set; }

    public uint Rule { get; set; }

    public string? ValueString { get; set; }

    public long? ValueInt { get; set; }

    public ulong? ValueUint { get; set; }

    public double? ValueFloat { get; set; }

    public uint Type { get; set; }

    public ulong? ValueUintExt { get; set; }
}
