using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5AntiddosSource
{
    public ulong SourceId { get; set; }

    public ulong Login { get; set; }

    public string From { get; set; } = null!;

    public string To { get; set; } = null!;
}
