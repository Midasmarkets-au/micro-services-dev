using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Daily2023
{
    public long Datetime { get; set; }

    public ulong Login { get; set; }

    public long Timestamp { get; set; }

    public long DatetimePrev { get; set; }

    public string Name { get; set; } = null!;

    public string Group { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string Company { get; set; } = null!;

    public string Email { get; set; } = null!;

    public double Balance { get; set; }

    public double Credit { get; set; }

    public double InterestRate { get; set; }

    public double CommissionDaily { get; set; }

    public double CommissionMonthly { get; set; }

    public double AgentDaily { get; set; }

    public double AgentMonthly { get; set; }

    public double BalancePrevDay { get; set; }

    public double BalancePrevMonth { get; set; }

    public double EquityPrevDay { get; set; }

    public double EquityPrevMonth { get; set; }

    public double Margin { get; set; }

    public double MarginFree { get; set; }

    public double MarginLevel { get; set; }

    public uint MarginLeverage { get; set; }

    public double Profit { get; set; }

    public double ProfitStorage { get; set; }

    public double ProfitEquity { get; set; }

    public double ProfitAssets { get; set; }

    public double ProfitLiabilities { get; set; }

    public double DailyProfit { get; set; }

    public double DailyBalance { get; set; }

    public double DailyCredit { get; set; }

    public double DailyCharge { get; set; }

    public double DailyCorrection { get; set; }

    public double DailyBonus { get; set; }

    public double DailyStorage { get; set; }

    public double DailyCommInstant { get; set; }

    public double DailyCommFee { get; set; }

    public double DailyCommRound { get; set; }

    public double DailyAgent { get; set; }

    public double DailyInterest { get; set; }

    public double DailyDividend { get; set; }

    public double DailyTaxes { get; set; }

    public double DailySocompensation { get; set; }

    public double DailySocompensationCredit { get; set; }
}
