using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt4Daily
{
    public int Login { get; set; }

    public DateTime Time { get; set; }

    public string Group { get; set; } = null!;

    public string Bank { get; set; } = null!;

    public double BalancePrev { get; set; }

    public double Balance { get; set; }

    public double Deposit { get; set; }

    public double Credit { get; set; }

    public double ProfitClosed { get; set; }

    public double Profit { get; set; }

    public double Equity { get; set; }

    public double Margin { get; set; }

    public double MarginFree { get; set; }

    public DateTime ModifyTime { get; set; }
}
