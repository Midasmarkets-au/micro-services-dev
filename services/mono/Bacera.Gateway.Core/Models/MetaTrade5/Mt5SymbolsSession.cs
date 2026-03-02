using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5SymbolsSession
{
    public ulong SessionId { get; set; }

    public ulong SymbolId { get; set; }

    public uint Type { get; set; }

    public uint Day { get; set; }

    public uint Open { get; set; }

    public uint Close { get; set; }
}
