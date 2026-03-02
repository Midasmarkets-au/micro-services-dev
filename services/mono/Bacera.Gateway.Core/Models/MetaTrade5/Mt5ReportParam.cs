using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5ReportParam
{
    public ulong ParamId { get; set; }

    public ulong Server { get; set; }

    public string Report { get; set; } = null!;

    public uint Type { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
