using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class ReferralCode
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int ServiceType { get; set; }

    public int IsDefault { get; set; }
    public short Status { get; set; }

    public long AccountId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public int IsAutoCreatePaymentMethod { get; set; }

    public virtual Party Party { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;
    public virtual ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}