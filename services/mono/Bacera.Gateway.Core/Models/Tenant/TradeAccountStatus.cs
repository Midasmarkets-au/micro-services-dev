using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TradeAccountStatus
{
    public long Id { get; set; }

    public int Leverage { get; set; }

    public int AgentAccount { get; set; }

    public double Balance { get; set; }

    public double PrevMonthBalance { get; set; }

    public double PrevBalance { get; set; }

    public double Credit { get; set; }

    public double InterestRate { get; set; }

    public double Taxes { get; set; }

    public double Equity { get; set; }

    public double Margin { get; set; }

    public double MarginLevel { get; set; }

    public double MarginFree { get; set; }

    public DateTime? LastLoginOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string ReadOnlyCode { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string? Group { get; set; }

    public virtual Account IdNavigation { get; set; } = null!;
    public virtual TradeAccount IdTradeNavigation { get; set; } = null!;
}