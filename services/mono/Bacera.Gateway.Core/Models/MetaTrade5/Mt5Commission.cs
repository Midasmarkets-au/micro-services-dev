using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Commission
{
    public ulong CommissionId { get; set; }

    public ulong GroupId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Path { get; set; } = null!;

    public uint Mode { get; set; }

    public uint ModeRange { get; set; }

    public uint ModeCharge { get; set; }

    public string TurnoverCurrency { get; set; } = null!;

    public uint ModeEntry { get; set; }

    public uint ModeAction { get; set; }

    public uint ModeProfit { get; set; }

    public uint ModeReason { get; set; }
}
