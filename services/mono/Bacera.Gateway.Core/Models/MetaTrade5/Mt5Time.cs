using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Time
{
    public int TimeZone { get; set; }

    public long Timestamp { get; set; }

    public string TimeServer { get; set; } = null!;

    public int Daylight { get; set; }

    public int DaylightState { get; set; }
}
