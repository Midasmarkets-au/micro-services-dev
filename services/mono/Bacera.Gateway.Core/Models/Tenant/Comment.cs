using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Comment
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public short Type { get; set; }

    public long RowId { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Content { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}
