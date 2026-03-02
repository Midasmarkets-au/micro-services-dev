using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Gateway
{
    public string Name { get; set; } = null!;

    public long Timestamp { get; set; }

    public string Module { get; set; } = null!;

    public string GatewayServer { get; set; } = null!;

    public string TradingServer { get; set; } = null!;

    public int Enable { get; set; }

    public int Flags { get; set; }

    public string Gateway { get; set; } = null!;

    public uint TimeoutReconnect { get; set; }

    public uint TimeoutSleep { get; set; }

    public uint AttempsSleep { get; set; }

    public ulong Id { get; set; }

    public string Symbols { get; set; } = null!;

    public uint SysConnection { get; set; }

    public DateTime SysLastTime { get; set; }

    public string Company { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public uint TickStatsCount { get; set; }

    public uint TicksCount { get; set; }

    public uint BooksCount { get; set; }

    public uint TradeAverageTime { get; set; }

    public uint TradeRequestsCount { get; set; }

    public uint BytesReceived { get; set; }

    public uint BytesSent { get; set; }

    public uint StateFlags { get; set; }
}
