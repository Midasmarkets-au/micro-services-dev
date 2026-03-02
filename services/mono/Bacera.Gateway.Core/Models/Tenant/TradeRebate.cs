using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TradeRebate
{
    public long Id { get; set; }

    public long? AccountId { get; set; }

    public int TradeServiceId { get; set; }

    public long Ticket { get; set; }

    public long AccountNumber { get; set; }

    public int CurrencyId { get; set; }

    public int Volume { get; set; }

    public int Status { get; set; }

    public int RuleType { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public DateTime ClosedOn { get; set; }

    public DateTime OpenedOn { get; set; }

    public long TimeStamp { get; set; }

    public int Action { get; set; }
    public long DealId { get; set; }

    public string Symbol { get; set; } = null!;
    public string ReferPath { get; set; } = null!;

    public double Commission { get; set; }
    public double Swaps { get; set; }
    public double OpenPrice { get; set; }
    public double ClosePrice { get; set; }
    public double Profit { get; set; }

    public int Reason { get; set; }
    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<Rebate> Rebates { get; set; } = new List<Rebate>();
    public virtual Account? Account { get; set; }

    public virtual TradeService TradeService { get; set; } = null!;
    public virtual ICollection<SalesRebate> SalesRebates { get; set; } = new List<SalesRebate>();
}