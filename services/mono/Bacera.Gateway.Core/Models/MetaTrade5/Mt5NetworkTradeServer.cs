using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5NetworkTradeServer
{
    public ulong Login { get; set; }

    public uint DemoMode { get; set; }

    public uint DemoPeriod { get; set; }

    public uint OvernightTime { get; set; }

    public uint OvernightMode { get; set; }

    public DateTime OvernighPrevTime { get; set; }

    public DateTime OvernightLastTime { get; set; }

    public uint OvernightDays { get; set; }

    public uint OvermonthMode { get; set; }

    public DateTime OvermonthPrevTime { get; set; }

    public DateTime OvermonthLastTime { get; set; }

    public uint TotalUsers { get; set; }

    public uint TotalUsersReal { get; set; }

    public uint TotalUsersApi { get; set; }

    public uint TotalDeals { get; set; }

    public uint TotalOrders { get; set; }

    public uint TotalOrdersHistory { get; set; }

    public uint TotalPositions { get; set; }

    public string LoginsRange { get; set; } = null!;

    public string LoginsRangeUsed { get; set; } = null!;

    public string OrdersRange { get; set; } = null!;

    public string OrdersRangeUsed { get; set; } = null!;

    public string DealsRange { get; set; } = null!;

    public string DealsRangeUsed { get; set; } = null!;
}
