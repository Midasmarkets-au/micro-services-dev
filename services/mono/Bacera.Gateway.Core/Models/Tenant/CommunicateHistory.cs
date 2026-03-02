using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class CommunicateHistory
{
    public long Id { get; set; }

    public int Type { get; set; }

    public long PartyId { get; set; }

    public long RowId { get; set; }

    public long OperatorPartyId { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Content { get; set; } = null!;

    public virtual Party OperatorParty { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}
