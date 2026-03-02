using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Account
{
    public ulong Login { get; set; }

    public uint CurrencyDigits { get; set; }

    public double Balance { get; set; }

    public double Credit { get; set; }

    public double Margin { get; set; }

    public double MarginFree { get; set; }

    public double MarginLevel { get; set; }

    public uint MarginLeverage { get; set; }

    public double MarginInitial { get; set; }

    public double MarginMaintenance { get; set; }

    public double Profit { get; set; }

    public double Storage { get; set; }

    public double Floating { get; set; }

    public double Equity { get; set; }

    public double Assets { get; set; }

    public double Liabilities { get; set; }

    public double BlockedCommission { get; set; }

    public double BlockedProfit { get; set; }
}
