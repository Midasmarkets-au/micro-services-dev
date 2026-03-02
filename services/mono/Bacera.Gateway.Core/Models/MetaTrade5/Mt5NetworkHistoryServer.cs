using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5NetworkHistoryServer
{
    public ulong Login { get; set; }

    public uint DatafeedTimeout { get; set; }

    public uint NewsMaxCount { get; set; }
}
