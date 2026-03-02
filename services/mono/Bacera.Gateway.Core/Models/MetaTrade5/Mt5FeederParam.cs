using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5FeederParam
{
    public ulong ParamId { get; set; }

    public string Feeder { get; set; } = null!;

    public uint Type { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
