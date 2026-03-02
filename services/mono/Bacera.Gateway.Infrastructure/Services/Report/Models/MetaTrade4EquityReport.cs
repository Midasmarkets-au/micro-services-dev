namespace Bacera.Gateway.Services.Report.Models;

public class MetaTrade4EquityReport
{
    // New	Previous Equity	Current Equity	Deposit	Withdraw	Transfer	Credit	Adjust	Agent	Net In/Out	P&L	Metal	Forex	Oil	Lots
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public int NewAccount { get; set; }
    public double Equity { get; set; }

    public double PreviousEquity { get; set; }
    public double Deposit { get; set; }
    public double Withdraw { get; set; }
    public double Transfer { get; set; }
    public double Adjust { get; set; }
    public double Credit { get; set; }
    public double Agent { get; set; }
    public double Metal { get; set; }
    public double Forex { get; set; }
    public double Oil { get; set; }
    public double Other { get; set; }

    public double NetInOut => Deposit + Withdraw + Transfer + Credit + Adjust + Agent;

    public double Pl => Equity - PreviousEquity - Deposit - Withdraw - Transfer - Credit - Adjust - Agent;

    public double Lots { get; set; }
}