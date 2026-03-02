using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5RoutingDealer
{
    public ulong Login { get; set; }

    public string RoutingName { get; set; } = null!;

    public string Name { get; set; } = null!;
}
