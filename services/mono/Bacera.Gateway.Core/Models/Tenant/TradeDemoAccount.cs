using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class TradeDemoAccount
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int ServiceId { get; set; }

    public long AccountNumber { get; set; }

    public DateTime ExpireOn { get; set; }

    public int Leverage { get; set; }

    public double Balance { get; set; }

    public short Type { get; set; }

    public int CurrencyId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public string ReferralCode { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual TradeService Service { get; set; } = null!;
}
