using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Firewall
{
    public string Ipfrom { get; set; } = null!;

    public string Ipto { get; set; } = null!;

    public long Timestamp { get; set; }

    public uint Action { get; set; }

    public string Comment { get; set; } = null!;
}
