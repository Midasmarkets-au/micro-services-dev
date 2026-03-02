using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateDirectSchemaItem
{
    public long Id { get; set; }

    public long RebateDirectSchemaId { get; set; }

    public decimal Rate { get; set; }

    public decimal Pips { get; set; }

    public decimal Commission { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string SymbolCode { get; set; } = null!;

    public virtual RebateDirectSchema RebateDirectSchema { get; set; } = null!;
}
