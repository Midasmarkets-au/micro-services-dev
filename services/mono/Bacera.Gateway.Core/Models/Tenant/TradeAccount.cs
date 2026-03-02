using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TradeAccount
{
    public long Id { get; set; }

    public int ServiceId { get; set; }

    public long AccountNumber { get; set; }

    public int CurrencyId { get; set; }

    public DateTime? LastSyncedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public long? RebateBaseSchemaId { get; set; }

    public virtual ICollection<CopyTrade> CopyTradeSourceAccounts { get; set; } = new List<CopyTrade>();

    public virtual ICollection<CopyTrade> CopyTradeTargetAccounts { get; set; } = new List<CopyTrade>();

    public virtual Currency Currency { get; set; } = null!;


    public virtual Account IdNavigation { get; set; } = null!;

    public virtual ICollection<RebateDirectRule> RebateDirectRules { get; set; } = new List<RebateDirectRule>();

    public virtual RebateBaseSchema? RebateBaseSchema { get; set; }

    public virtual TradeService Service { get; set; } = null!;

    public virtual TradeAccountStatus? TradeAccountStatus { get; set; }
}