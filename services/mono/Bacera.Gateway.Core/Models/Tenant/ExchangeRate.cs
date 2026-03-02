using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class ExchangeRate
{
    public long Id { get; set; }

    public int FromCurrencyId { get; set; }

    public int ToCurrencyId { get; set; }

    public decimal BuyingRate { get; set; }

    public decimal SellingRate { get; set; }

    public decimal AdjustRate { get; set; }

    public DateTime UpdatedOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Name { get; set; } = null!;

    public virtual Currency FromCurrency { get; set; } = null!;

    public virtual Currency ToCurrency { get; set; } = null!;
}
