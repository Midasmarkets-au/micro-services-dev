using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TradeService
{
    public int Id { get; set; }

    public short Platform { get; set; }

    public int Priority { get; set; }

    public short IsAllowAccountCreation { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public string Configuration { get; set; } = null!;

    public virtual ICollection<TradeAccount> TradeAccounts { get; set; } = new List<TradeAccount>();

    public virtual ICollection<TradeDemoAccount> TradeDemoAccounts { get; set; } = new List<TradeDemoAccount>();

    public virtual ICollection<TradeRebate> TradeRebates { get; set; } = new List<TradeRebate>();
}
