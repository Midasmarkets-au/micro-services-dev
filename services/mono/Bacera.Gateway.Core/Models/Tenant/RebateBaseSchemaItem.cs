using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class RebateBaseSchemaItem
{
    public long Id { get; set; }

    public long RebateBaseSchemaId { get; set; }

    public decimal Rate { get; set; }

    public decimal Pips { get; set; }

    public decimal Commission { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string SymbolCode { get; set; } = null!;

    public virtual RebateBaseSchema RebateBaseSchema { get; set; } = null!;
}