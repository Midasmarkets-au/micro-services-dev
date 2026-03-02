using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Feeder
{
    public string Feeder { get; set; } = null!;

    public long Timestamp { get; set; }

    public string Module { get; set; } = null!;

    public string GatewayServer { get; set; } = null!;

    public string FeedServer { get; set; } = null!;

    public int Enable { get; set; }

    public int Mode { get; set; }

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

    public uint NewsCount { get; set; }

    public uint BytesReceived { get; set; }

    public uint BytesSent { get; set; }

    public uint StateFlags { get; set; }
}
