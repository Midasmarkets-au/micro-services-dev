using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Referral
{
    public long Id { get; set; }

    public long RowId { get; set; }

    public long ReferralCodeId { get; set; }

    public long ReferrerPartyId { get; set; }

    public long ReferredPartyId { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Code { get; set; } = null!;

    public string Module { get; set; } = null!;

    public virtual ReferralCode ReferralCode { get; set; } = null!;

    public virtual Party ReferredParty { get; set; } = null!;

    public virtual Party ReferrerParty { get; set; } = null!;
}
