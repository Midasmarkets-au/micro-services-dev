using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5CommissionsTier
{
    public ulong TierId { get; set; }

    public ulong CommissionId { get; set; }

    public uint Mode { get; set; }

    public uint Type { get; set; }

    public double Value { get; set; }

    public double RangeFrom { get; set; }

    public double RangeTo { get; set; }

    public double Minimal { get; set; }

    public double Maximal { get; set; }

    public string Currency { get; set; } = null!;
}
