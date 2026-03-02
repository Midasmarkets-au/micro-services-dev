using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Bacera.Gateway;

public partial class AccountStat
{
    public long Id { get; set; }
    public long AccountId { get; set; }

    public DateTime Date { get; set; }

    public long NewAccountCount { get; set; }
    public long NewAgentCount { get; set; }
    public long DepositAmount { get; set; }
    public long DepositCount { get; set; }
    public long WithdrawAmount { get; set; }
    public long WithdrawCount { get; set; }
    public long TradeVolume { get; set; }
    public string TradeSymbol { get; set; } = null!;
    public long RebateAmount { get; set; }
    public long RebateCount { get; set; }

    public long SalesRebateAmount { get; set; }
    public long SalesRebateCount { get; set; }

    public long Credit { get; set; }
    public long Adjust { get; set; }
    public long Equity { get; set; }
    public long PreviousEquity { get; set; }

    public long TradeProfit { get; set; }
    public long TradeCount { get; set; }
    public virtual Account Account { get; set; } = null!;
}